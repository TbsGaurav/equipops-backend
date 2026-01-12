using Common.Services.Services.Interface;
using Common.Services.ViewModels.ResumeParse;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Services.Implementation;

public class ResumeParseService : IResumeParseService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ResumeParseService> _logger;

    public ResumeParseService(HttpClient httpClient, ILogger<ResumeParseService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    #region Single Resume Parse
    public async Task<ResumeParseResult> ParseResumeAsync(string resumeText, string apiKey)
    {
        var prompt = BuildResumePrompt(resumeText);

        var body = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                    new { role = "system", content = "You are a strict JSON generator." },
                    new { role = "user", content = prompt }
                },
            temperature = 0.1,
            max_tokens = 1200
        };

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await _httpClient.PostAsJsonAsync(
            "https://api.openai.com/v1/chat/completions",
            body);

        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadAsStringAsync();
        var content = ExtractAssistantContent(raw);
        var jsonOnly = ExtractJson(content);

        ResumeParseResult resumeParseResult = new();
        try
        {
            var result = JsonSerializer.Deserialize<ResumeProfile>(jsonOnly,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

            resumeParseResult = new ResumeParseResult()
            {
                Profile = result,
                Success = true,
                Error = string.Empty
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex,
                "Failed to deserialize OpenAI JSON. JSON: {Json}", jsonOnly);

            resumeParseResult = new ResumeParseResult()
            {
                Profile = null,
                Success = false,
                Error = ex.Message
            };

            throw;
        }

        return resumeParseResult;
    }
    private static string BuildResumePrompt(string resumeText)
    {
        return $@"
                    You are an expert resume parser.

                    Analyze the resume text below and extract structured information.

                    IMPORTANT RULES:
                    - Resume can belong to ANY department (IT, Sales, Mechanical, Electrical, Electronics, HR, Finance, etc.)
                    - If a field is missing, return null
                    - Do NOT guess salary or experience
                    - Return ONLY valid JSON
                    - NO markdown
                    - NO explanations

                    TASKS:
                    1. Identify candidate department/domain
                    2. Extract total years of experience
                    3. Extract technical skills and soft skills
                    4. Extract education details
                    5. Extract current and expected salary if mentioned
                    6. Generate a short professional summary

                    EXPECTED JSON FORMAT:
                    {{
                      ""fullName"": ""string"",
                      ""email"": ""string"",
                      ""phone"": ""string"",
                      ""department"": ""string"",
                      ""totalExperienceYears"": 0,
                      ""skills"": [""skill1"", ""skill2""],
                      ""softSkills"": {{
                        ""communication"": 1-10,
                        ""teamwork"": 1-10,
                        ""leadership"": 1-10
                      }},
                      ""experienceYears"": number,
                      ""education"": [""Degree / Certification""],
                      ""currentCompany"": ""string | null"",
                      ""currentRole"": ""string | null"",
                      ""currentSalary"": null,
                      ""expectedSalary"": null,
                      ""summary"": ""string""
                    }}

                    RESUME TEXT:
                    {resumeText.Trim()}";
    }
    #endregion

    #region Batch Resume Parse
    public async Task<List<ResumeParseResult>> ParseResumesBatchAsync(List<string> resumes, string azureApiKey)
    {
        var results = new ConcurrentBag<ResumeParseResult>();
        var semaphore = new SemaphoreSlim(4); // Azure rate-limit safe

        var tasks = resumes.Select(async resume =>
        {
            await semaphore.WaitAsync();
            try
            {
                var parsed = await ParseResumeAsync(
                    resume,
                    azureApiKey);

                results.Add(new ResumeParseResult
                {
                    Success = true,
                    Profile = parsed.Profile
                });
            }
            catch (Exception ex)
            {
                results.Add(new ResumeParseResult
                {
                    Success = false,
                    Error = ex.Message
                });
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return results.ToList();
    }

    public async Task<ResumeProfile> ParseResumeAsyncBatch(string resumeText, string openAiApiKey, CancellationToken ct)
    {
        var body = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "You are a resume parser. Output must strictly match the JSON schema."
                },
                new
                {
                    role = "user",
                    content = resumeText
                }
            },
            temperature = 0,
            max_tokens = 1200,
            response_format = new
            {
                type = "json_schema",
                json_schema = ResumeJsonSchema.Schema
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", openAiApiKey);

        request.Content = JsonContent.Create(body);

        using var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadAsStringAsync(ct);

        var content = ExtractAssistantContent(raw);
        var jsonOnly = ExtractJson(content);

        // 🔹 Apply converter HERE
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new StringOrArrayConverter());

        return JsonSerializer.Deserialize<ResumeProfile>(
            jsonOnly!,
            options
        )!;
    }
    #endregion

    #region static function
    private static string ExtractJson(string input)
    {
        input = input.Replace("```json", "").Replace("```", "").Trim();

        var start = input.IndexOf('{');
        var end = input.LastIndexOf('}');

        if (start < 0 || end < 0)
            throw new Exception("Invalid JSON returned");

        return input.Substring(start, end - start + 1);
    }
    private static string ExtractAssistantContent(string response)
    {
        using var doc = JsonDocument.Parse(response);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }
    public static class ResumeJsonSchema
    {
        public static object Schema => new
        {
            name = "resume_parse",
            schema = new
            {
                type = "object",
                properties = new
                {
                    fullName = new { type = new[] { "string", "null" } },
                    email = new { type = new[] { "string", "null" } },
                    phone = new { type = new[] { "string", "null" } },
                    department = new { type = new[] { "string", "null" } },
                    totalExperienceYears = new { type = new[] { "number", "null" } },
                    skills = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    softSkills = new
                    {
                        type = "object",
                        properties = new
                        {
                            communication = new
                            {
                                type = new[] { "integer", "null" },
                                minimum = 1,
                                maximum = 10,
                                description = "Score from 1 (very low) to 10 (excellent)"
                            },
                            teamwork = new
                            {
                                type = new[] { "integer", "null" },
                                minimum = 1,
                                maximum = 10,
                                description = "Score from 1 (very low) to 10 (excellent)"
                            },
                            leadership = new
                            {
                                type = new[] { "integer", "null" },
                                minimum = 1,
                                maximum = 10,
                                description = "Score from 1 (very low) to 10 (excellent)"
                            }
                        }
                    },
                    education = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    currentCompany = new { type = new[] { "string", "null" } },
                    currentRole = new { type = new[] { "string", "null" } },
                    currentSalary = new { type = new[] { "number", "null" } },
                    expectedSalary = new { type = new[] { "number", "null" } },
                    summary = new { type = new[] { "string", "null" } }
                },
                required = new[] { "skills", "education" }
            }
        };
    }
    #endregion
}
