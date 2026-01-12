using Common.Services.Services.Interface;
using Common.Services.ViewModels.General;

using Microsoft.Extensions.Logging;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Services.Implementation
{
    public class GeneralAIService : IGeneralAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeneralAIService> _logger;

        public GeneralAIService(HttpClient httpClient, ILogger<GeneralAIService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IndustryDepartmentResponse> GetDepartmensByIndustryAsync(string industry, string apiKey)
        {
            var prompt = BuildDepartmentPrompt(industry);

            var body = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "You are a deterministic JSON generator." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.0,   // 🔥 CRITICAL
                top_p = 1.0,
                max_tokens = 400
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

            IndustryDepartmentResponse result;

            try
            {
                result = JsonSerializer.Deserialize<IndustryDepartmentResponse>(jsonOnly,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "Failed to deserialize OpenAI JSON. JSON: {Json}", jsonOnly);
                throw;
            }

            return result;
        }

        public async Task<JobObjectiveResponse> GetJobObjectivesAsync(string apiKey, string jobType, string workMode, int experienceYears, string objective)
        {
            _logger.LogInformation(
                "Generating Job Objective | JobType={JobType}, WorkMode={WorkMode}, Experience={Experience}",
                jobType, workMode, experienceYears);

            var prompt = BuildJobObjectivePrompt(jobType, workMode, experienceYears, objective);

            var body = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "You generate stable, deterministic text." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.0,   // 🔥 NO randomness
                top_p = 1.0,
                max_tokens = 600
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsJsonAsync(
                "https://api.openai.com/v1/chat/completions", body);

            response.EnsureSuccessStatusCode();

            var rawResponse = await response.Content.ReadAsStringAsync();
            var jobObjective = ExtractAssistantContent(rawResponse);

            try
            {
                JobObjectiveResponse jobObjectiveResponse = new()
                {
                    JobObjective = jobObjective
                };

                _logger.LogInformation("Job Objective generated successfully");

                return jobObjectiveResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to parse Job Objective JSON. Raw JSON: {jobObjective}", jobObjective);
                throw;
            }
        }


        #region Private Method
        private static string BuildDepartmentPrompt(string industry)
        {
            return $@"
                        You are an HR domain expert.

                        TASK:
                        Given an industry name, return a STANDARDIZED list of COMMON departments used in that industry.

                        IMPORTANT RULES (MUST FOLLOW):
                        - Departments must be REALISTIC and INDUSTRY-STANDARD
                        - Use commonly accepted department names only
                        - Do NOT invent rare or niche departments
                        - Return EXACTLY 8 departments
                        - Sort departments ALPHABETICALLY
                        - Same industry MUST always return the SAME departments
                        - Industry can be ANY (IT, Healthcare, Manufacturing, Finance, Education, Retail, Construction, HR, Accounting, Mechanical, Electronics, etc.)

                        OUTPUT RULES:
                        - Return ONLY valid JSON
                        - No markdown
                        - No explanations
                        - No extra text

                        EXPECTED JSON FORMAT:
                        {{
                          ""industry"": ""{industry}"",
                          ""departments"": [
                            ""Department 1"",
                            ""Department 2"",
                            ""Department 3"",
                            ""Department 4"",
                            ""Department 5"",
                            ""Department 6"",
                            ""Department 7"",
                            ""Department 8""
                          ]
                        }}

                        Industry:
                        {industry}
                        ";
        }


        private static string BuildJobObjectivePrompt(string jobType, string workMode, int experienceYears, string baseObjective)
        {
            return $@"
                        You are a professional HR job-content writer.

                        Your task is to GENERATE a JOB OBJECTIVE by refining and expanding the provided base objective.

                        INPUT PARAMETERS:
                        - Job Type: {jobType}
                        - Work Mode: {workMode}
                        - Experience Years: {experienceYears}
                        - Base Job Objective: {baseObjective}

                        STRICT OUTPUT RULES:
                        - Output MUST be plain text only
                        - NO JSON
                        - NO markdown
                        - NO bullet points
                        - NO numbering
                        - EXACTLY 10 to 15 lines
                        - Each line MUST be a complete professional sentence
                        - Do NOT mention company names
                        - Do NOT mention candidate names
                        - Do NOT include responsibilities
                        - Do NOT include benefits or salary
                        - Same input MUST always produce the same output

                        EXPERIENCE-BASED DEPTH:
                        - If experienceYears <= 2:
                          Focus on role foundation, structured learning, operational exposure, and skill alignment
                        - If experienceYears between 3 and 6:
                          Focus on execution quality, ownership, cross-functional collaboration, and consistency
                        - If experienceYears >= 7:
                          Focus on leadership scope, strategic alignment, domain depth, mentoring, and long-term impact

                        CONTENT RULES:
                        - This is a JOB OBJECTIVE, not a candidate summary
                        - The wording must describe the purpose and expectations of the role
                        - Naturally include job type and work mode in the narrative
                        - Must remain generic and applicable across industries (IT, HR, Finance, Mechanical, Sales, etc.)
                        - Expand logically from the base objective without changing its intent

                        Generate the job objective now.
                        ";
        }


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
        #endregion
    }
}
