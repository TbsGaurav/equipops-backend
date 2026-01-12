using Common.Services.ViewModels.InterviewEvaluation;

using InterviewService.Api.Helpers.ResponseHelpers.Enums;
using InterviewService.Api.Helpers.ResponseHelpers.Handlers;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Response.JobDetail;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace InterviewService.Api.Services.Implementation
{
    public class JobAnalysisService : IJobAnalysisService
    {
        private readonly IJobAnalysisRepository _iJobAnalysisRepository;
        private readonly ILogger<JobAnalysisService> _logger;

        public JobAnalysisService(IJobAnalysisRepository iJobAnalysisRepository, ILogger<JobAnalysisService> logger)
        {
            _iJobAnalysisRepository = iJobAnalysisRepository;
            _logger = logger;
        }

        public async Task<IActionResult> GetJobAnalysisAsync(Guid id)
        {
            _logger.LogInformation(
                "API Request: Fetching job analysis for InterviewId: {InterviewId}",
                id
            );

            var result = await _iJobAnalysisRepository.GetJobAnalysisAsync(id);

            if (result == null)
            {
                _logger.LogWarning("Job analysis not found for InterviewId: {InterviewId}", id);

                return new NotFoundObjectResult(
                    ResponseHelper<JobAnalysisResponseViewModel>.Error(
                        "Job analysis not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    )
                );
            }

            _logger.LogInformation(
                "Job analysis retrieved successfully for InterviewId: {InterviewId}",
                id
            );

            return new OkObjectResult(
                ResponseHelper<JobAnalysisResponseViewModel>.Success(
                    "Job analysis fetched successfully.",
                    result
                )
            );
        }

        public async Task<IActionResult> GetCandidateJobAnalysis(Guid id)
        {
            _logger.LogInformation(
                "API Request: Fetching job analysis for InterviewId: {InterviewId}",
                id
            );

            var result = await _iJobAnalysisRepository.GetCandidateJobAnalysis(id);

            if (result == null)
            {
                _logger.LogWarning("Job analysis not found for InterviewId: {InterviewId}", id);

                return new NotFoundObjectResult(
                    ResponseHelper<CandidateJobAnalysisResponseViewModel>.Error(
                        "Job analysis not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    )
                );
            }

            _logger.LogInformation(
                "Job analysis retrieved successfully for InterviewId: {InterviewId}",
                id
            );


            var questionEvaluations = JsonConvert.DeserializeObject<List<QuestionEvaluation>>(result?.QuestionsEvaluation?.QuestionsEvaluation ?? "[]");

            result.InterviewQuestionSummary = questionEvaluations
                                            .Select(q => new InterviewQuestionSummary
                                            {
                                                InterviewId = result.InterviewInfo?.InterviewId ?? Guid.Empty,
                                                Question = q.Question,
                                                Answer = q.Answer,
                                                AnswerStatus = q.Answer_Status
                                            })
                                            .ToList();

            result.interviewTranscript = new InterviewTranscriptViewModel()
            {
                InterviewId = result?.InterviewInfo?.InterviewId ?? Guid.Empty,
                Messages = ParseTranscript(result?.QuestionsEvaluation?.Transcript ?? string.Empty)
            };

            return new OkObjectResult(
                ResponseHelper<CandidateJobAnalysisResponseViewModel>.Success(
                    "Candidate job analysis fetched successfully.",
                    result
                )
            );
        }

        private static List<InterviewTranscriptMessage> ParseTranscript(string transcript)
        {
            if (string.IsNullOrWhiteSpace(transcript))
                return [];

            var messages = new List<InterviewTranscriptMessage>();

            TranscriptSpeaker? currentSpeaker = null;
            bool isInCodeBlock = false;
            string? codeLanguage = null;

            var buffer = new List<string>();

            var lines = transcript.Split('\n');

            void FlushBuffer()
            {
                if (currentSpeaker == null || buffer.Count == 0)
                    return;

                messages.Add(new InterviewTranscriptMessage
                {
                    Speaker = currentSpeaker.Value,
                    Message = string.Join("\n", buffer).Trim(),
                    IsCodeBlock = isInCodeBlock,
                    Language = codeLanguage
                });

                buffer.Clear();
                isInCodeBlock = false;
                codeLanguage = null;
            }

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd();

                // Detect start or end of code block
                if (line.StartsWith("```"))
                {
                    if (!isInCodeBlock)
                    {
                        // Starting code block
                        FlushBuffer();
                        isInCodeBlock = true;
                        codeLanguage = line.Replace("```", "").Trim();
                        continue;
                    }
                    else
                    {
                        // Ending code block
                        FlushBuffer();
                        continue;
                    }
                }

                // Speaker detection (only if NOT inside code)
                if (!isInCodeBlock &&
                    line.StartsWith("agent:", StringComparison.OrdinalIgnoreCase))
                {
                    FlushBuffer();
                    currentSpeaker = TranscriptSpeaker.Agent;
                    buffer.Add(line.Substring(6).Trim());
                    continue;
                }

                if (!isInCodeBlock &&
                    line.StartsWith("user:", StringComparison.OrdinalIgnoreCase))
                {
                    FlushBuffer();
                    currentSpeaker = TranscriptSpeaker.User;
                    buffer.Add(line.Substring(5).Trim());
                    continue;
                }

                // Normal content or code content
                buffer.Add(line);
            }

            FlushBuffer();

            NormalizeSilence(messages);

            return messages;
        }

        private static void NormalizeSilence(List<InterviewTranscriptMessage> messages)
        {
            foreach (var msg in messages)
            {
                var text = msg.Message?.Trim().ToLowerInvariant();

                if (text is "(inaudible speech)" or "(inaudible)" or "(silence)" or "(noise)")
                {
                    msg.Message = string.Empty;
                }
            }
        }
    }
}
