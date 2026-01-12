using Common.Services.Services.Interface;
using Common.Services.ViewModels.InterviewEvaluation;

using Microsoft.Extensions.Logging;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Services.Implementation
{
    public class InterviewEvaluationService : IInterviewEvaluationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InterviewEvaluationService> _logger;

        public InterviewEvaluationService(HttpClient httpClient, ILogger<InterviewEvaluationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<InterviewEvaluationResponse> EvaluateAsync(InterviewTranscriptRequest request)
        {
            _logger.LogInformation("Interview evaluation started");

            if (string.IsNullOrWhiteSpace(request?.key))
            {
                _logger.LogWarning("API key is missing");
                return RejectResponse();
            }

            if (string.IsNullOrWhiteSpace(request?.Transcript))
            {
                _logger.LogWarning("Transcript is empty or null");
                return RejectResponse();
            }

            try
            {
                var prompt = BuildPrompt(request);

                var body = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                    new { role = "system", content = "You are a strict JSON generator." },
                    new { role = "user", content = prompt }
                },
                    temperature = 0.2,
                    max_tokens = 1200
                };

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", request.key);

                _logger.LogInformation("Sending request to OpenAI");

                var response = await _httpClient.PostAsJsonAsync(
                    "https://api.openai.com/v1/chat/completions",
                    body);

                response.EnsureSuccessStatusCode();

                var rawResponse = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Raw OpenAI response received");

                var assistantContent = ExtractAssistantContent(rawResponse);
                var jsonOnly = ExtractJson(assistantContent);

                InterviewEvaluationResponse result;

                try
                {
                    result = JsonSerializer.Deserialize<InterviewEvaluationResponse>(
                        jsonOnly,
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

                CalculateOverallScores(result);

                _logger.LogInformation(
                    "Interview evaluation completed. OverallScore={Score}, Decision={Decision}",
                    result.OverallScores.Overall,
                    result.HiringDecision);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Interview evaluation failed");
                throw;
            }
        }
        private static string BuildPrompt1(InterviewTranscriptRequest req)
        {
            return $@"
                        You are a senior professional interviewer and evaluator.

                        Analyze the interview transcript below and produce a FINAL evaluation.

                        The interview may belong to ANY domain such as:
                        - Software / IT
                        - Mechanical
                        - Electrical / Electronics
                        - HR
                        - Sales
                        - Accounting / Finance
                        - Operations or other professional fields

                        IMPORTANT INTERPRETATION RULES:
                        - If the candidate provides SOURCE CODE (any language), treat it as a VALID answer.
                        - If code is present:
                          - Validate syntax, correctness, logic, and best practices for that domain.
                        - NEVER mark an answer as ""inaudible"" if meaningful content or code is present.
                        - If answers are unclear AND no meaningful explanation or code is provided, reduce scores.
                        - Judge answers according to the interview's DOMAIN (not only technical coding).

                        EVALUATION CRITERIA (0–10):
                        - Technical / Domain Knowledge
                        - Communication
                        - Confidence

                        SCORING SCALE:
                        - All skill scores MUST be between 0 and 10.

                        EVALUATION CRITERIA:
                        - Technical Skill
                        - Communication
                        - Confidence

                        TASKS:
                        1. Analyze all candidate responses collectively (not question-wise).
                        2. Evaluate correctness based on the interview domain.
                        3. Validate any provided examples or code if applicable.
                        4. Calculate average scores for:
                           - technical
                           - communication
                           - confidence
                        5. Calculate overall score as the average of all skills.
                        6. Convert overall score to percentage using:
                           percentage = (overall / 10) * 100
                        7. Decide hiring outcome:
                           - Hired (percentage >= 70)
                           - Shortlisted (percentage >= 50 and < 70)
                           - Rejected (percentage < 50)
                        8. Provide a clear hiring reason explaining WHY the decision was made.

                        STRICT OUTPUT RULES:
                        - Return ONLY valid JSON
                        - NO markdown
                        - NO explanations outside JSON
                        - DO NOT include questions or per-question evaluations
                        - NEVER truncate output
                        - Do NOT return null values (use 0 instead)
                       
                        EXPECTED JSON FORMAT:
                        {{
                          ""overallScores"": {{
                            ""technical"": 0,
                            ""communication"": 0,
                            ""confidence"": 0,
                            ""overall"": 0,
                            ""percentage"": 0
                          }},
                          ""hiringDecision"": ""Hired | Shortlisted | Rejected"",
                          ""hiringReason"": ""Clear explanation for the decision""
                        }}

                        Transcript:{req.Transcript.Trim()}";
        }

        private static string BuildPrompt(InterviewTranscriptRequest req)
        {
            return $@"
                        You are a senior professional interviewer and evaluator.

                        Analyze the interview transcript below and produce a QUESTION-WISE evaluation.

                        The interview may belong to ANY domain such as:
                        - Software / IT
                        - Mechanical
                        - Electrical / Electronics
                        - HR
                        - Sales
                        - Accounting / Finance
                        - Operations or other professional fields

                        IMPORTANT INTERPRETATION RULES:
                        - Identify each distinct interviewer question.
                        - Match the most relevant candidate response to each question.
                        - If the candidate provides SOURCE CODE (any language), treat it as a VALID answer.
                        - If code is present:
                          - Validate syntax, correctness, logic, and best practices for that domain.
                        - NEVER mark an answer as ""Not Answered"" if meaningful explanation or code is present.
                        - Judge answers according to the interview's DOMAIN (not only technical coding).
                        - If the answer is unclear AND no meaningful explanation or code is provided, mark as ""Not Answered"".

                        ANSWER STATUS RULES (PER QUESTION):
                        Determine `answer_status` strictly as one of the following:
                        - ""Correct"":
                          - Fully and correctly answers the question
                          - Correct explanation, example, or working code
                        - ""Partial"":
                          - Shows some understanding
                          - Misses key points or has minor mistakes
                          - Incomplete explanation or partially correct code
                        - ""Not Answered"":
                          - No meaningful or relevant response
                          - Candidate says ""I don't know"" or answer is unrelated
                          - No usable explanation or code

                        SCORING RULES (PER QUESTION):
                        - Scores range from 0–10
                        - If `answer_status` is ""Not Answered"":
                          - technical = 0
                          - communication = 0
                          - confidence = 0
                        - If `answer_status` is ""Partial"":
                          - Use mid-range values
                        - If `answer_status` is ""Correct"":
                          - Use higher-range values

                        EVALUATION CRITERIA (0–10):
                        - Technical / Domain Knowledge
                        - Communication
                        - Confidence

                        TASKS:
                        1. Identify all interviewer questions.
                        2. Evaluate each question independently.
                        3. Assign `answer_status` per question.
                        4. Score each answer for:
                           - technical
                           - communication
                           - confidence
                        5. Calculate overall averages across all questions:
                           - technical
                           - communication
                           - confidence
                        6. Calculate overall score as the average of all skills.
                        7. Convert overall score to percentage using:
                           percentage = (overall / 10) * 100
                        8. Decide hiring outcome:
                           - Hired (percentage >= 70)
                           - Shortlisted (percentage >= 50 and < 70)
                           - Rejected (percentage < 50)
                        9. Provide a clear hiring reason explaining WHY the decision was made.

                        STRICT OUTPUT RULES:
                        - Return ONLY valid JSON
                        - NO markdown
                        - NO explanations outside JSON
                        - NEVER truncate output
                        - Do NOT return null values (use 0 instead)

                        EXPECTED JSON FORMAT:
                        {{
                          ""evaluations"": [
                            {{
                              ""question"": ""string"",
                              ""answer"": ""string"",
                              ""answer_status"": ""Correct | Partial | Not Answered"",
                              ""technical"": 0,
                              ""communication"": 0,
                              ""confidence"": 0
                            }}
                          ],
                          ""overallScores"": {{
                            ""technical"": 0,
                            ""communication"": 0,
                            ""confidence"": 0,
                            ""overall"": 0,
                            ""percentage"": 0
                          }},
                          ""hiringDecision"": ""Hired | Shortlisted | Rejected"",
                          ""hiringReason"": ""Clear explanation for the decision""
                        }}

                            Transcript:
                        {req.Transcript.Trim()}
                        ";
        }


        private static string ExtractJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Empty response from OpenAI");

            input = input
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var start = input.IndexOf('{');
            var end = input.LastIndexOf('}');

            if (start < 0 || end < 0 || end <= start)
                throw new InvalidOperationException("No valid JSON found");

            return input.Substring(start, end - start + 1);
        }
        private static void CalculateOverallScores(InterviewEvaluationResponse response)
        {
            if (response?.OverallScores == null)
            {
                response.OverallScores = new OverallSkillScores();
                response.HiringDecision = "Reject";
                response.HiringReason = "No valid evaluation data returned.";
                return;
            }

            // Safety clamp (0–5)
            response.OverallScores.Technical = Clamp(response.OverallScores.Technical);
            response.OverallScores.Communication = Clamp(response.OverallScores.Communication);
            response.OverallScores.Confidence = Clamp(response.OverallScores.Confidence);
            response.OverallScores.Overall = Clamp(response.OverallScores.Overall);

            // Auto decision fallback if OpenAI missed it
            if (string.IsNullOrWhiteSpace(response.HiringDecision))
            {
                CalculateHiringDecision(response);
            }
        }
        private static double Clamp(double value)
        {
            if (value < 0) return 0;
            if (value > 10) return 10;
            return Math.Round(value, 2);
        }
        private static string ExtractAssistantContent(string openAiResponse)
        {
            using var doc = JsonDocument.Parse(openAiResponse);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;
        }
        private static void CalculateHiringDecision(InterviewEvaluationResponse response)
        {
            var percentage = (response.OverallScores.Overall / 10.0) * 100;
            response.OverallScores.Percentage = Math.Round(percentage, 2);

            if (percentage >= 70)
                response.HiringDecision = "Hire";
            else if (percentage >= 50)
                response.HiringDecision = "Shortlist";
            else
                response.HiringDecision = "Reject";
        }
        private static InterviewEvaluationResponse RejectResponse()
        {
            return new InterviewEvaluationResponse
            {
                OverallScores = new OverallSkillScores(),
                HiringDecision = "Reject"
            };
        }
    }
}
