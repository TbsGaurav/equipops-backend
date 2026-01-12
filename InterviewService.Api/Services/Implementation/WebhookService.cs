using InterviewService.Api.Helpers.ResponseHelpers.Handlers;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interview_transcript;
using InterviewService.Api.ViewModels.Response.Interview_transcript;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace InterviewService.Api.Services.Implementation
{
    public class WebhookService(ILogger<WebhookService> _logger, IInterviewTranscriptRepository interviewTranscriptRepository, IWebHookRepository _webHookRepository, IInterviewTranscriptService interviewTranscriptService, IInterviewRepository interviewRepository) : IWebhookService
    {
        public async Task<IActionResult> HandleRetellEventAsync(string eventName, string callId, JsonElement body)
        {
            try
            {

                _logger.LogInformation("Received Retell event: {eventName}", eventName);

                switch (eventName)
                {
                    case "call_started":
                        await _webHookRepository.HandleCallStartedAsync(callId);
                        break;

                    case "call_ended":
                        await _webHookRepository.HandleCallEndedAsync(callId, body);
                        break;

                    case "call_analyzed":
                        await HandleCallAnalyzedAsync(callId, body, openAiKey: "");
                        break;

                    default:
                        _logger.LogWarning("Unhandled Retell event: {eventName}", eventName);
                        break;
                }
                return new OkObjectResult(ResponseHelper<string>.Success("Webhook processed successfully", "true"));
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task HandleCallAnalyzedAsync(string callId, JsonElement body, string openAiKey)
        {
            _logger.LogInformation("HandleCallAnalyzedAsync START | CallId={CallId}", callId);

            if (string.IsNullOrWhiteSpace(callId))
                return;

            // call object
            if (!body.TryGetProperty("call", out var callProp) ||
                callProp.ValueKind != JsonValueKind.Object)
            {
                _logger.LogWarning("Call object not found. CallId={CallId}", callId);
                return;
            }

            // ✅ Retell transcript lives here
            if (!callProp.TryGetProperty("transcript_object", out var transcriptArray) ||
                transcriptArray.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("Transcript object not found. CallId={CallId}", callId);
                return;
            }

            var interviewData =
                await _webHookRepository.GetCandidateInterviewByCallIdAsync(callId);

            if (interviewData == null)
            {
                _logger.LogWarning("Interview not found. CallId={CallId}", callId);
                return;
            }

            var transcriptItems = new List<RetellTranscriptItem>();
            var conversationBuilder = new StringBuilder();
            foreach (var segment in transcriptArray.EnumerateArray())
            {
                if (segment.ValueKind != JsonValueKind.Object)
                    continue;

                segment.TryGetProperty("role", out var roleProp);
                segment.TryGetProperty("content", out var contentProp);
                if (contentProp.ValueKind == JsonValueKind.Undefined)
                    segment.TryGetProperty("text", out contentProp);

                var role = roleProp.GetString()?.ToLower();
                var content = contentProp.GetString();

                if (string.IsNullOrWhiteSpace(content))
                    continue;
                var normalizedRole = role == "agent" ? "Interviewer" : "User";

                transcriptItems.Add(new RetellTranscriptItem
                {
                    Role = role == "agent" ? "Interviewer" : "User",
                    Content = content
                });
                conversationBuilder.AppendLine($"{normalizedRole}: {content}");
            }
            var conversationText = conversationBuilder.ToString();
            if (!transcriptItems.Any())
            {
                _logger.LogWarning("No valid transcript content. CallId={CallId}", callId);
                return;
            }
            var interviewerData = await interviewRepository.GetInterviewByIdAsync(interviewData.InterviewId);
            var interviewerId = interviewerData.Interview.Interviewer_Id;

            // Build request
            var UpdateRequest = new InterviewTrasncriptCreateRequestViewModel
            {
                Call_id = callId,
                Interview_id = interviewData.InterviewId,
                Candidate_id = interviewData.CandidateId,
                Interviewer_id = interviewerId,
                Organization_id = interviewData.OrganizationId
            };
            var callResponse = new RetellGetCallResponse
            {
                TranscriptObject = transcriptItems
            };

            var (interviewUpdateId, evaluationResult) = await interviewTranscriptService.ProcessAndUpdateInterviewTranscriptAsync(
                    callResponse, UpdateRequest, openAiKey);
              // Build request
            var transcriptRequest = new InterviewTrasncriptCreateRequestViewModel
            {
                interview_update_id = !string.IsNullOrWhiteSpace(interviewUpdateId) ? Guid.Parse(interviewUpdateId) : null,
                Call_id = callId,
                Interview_id = interviewData.InterviewId,
                Candidate_id = interviewData.CandidateId,
                Interviewer_id = interviewerId,
                Organization_id = interviewData.OrganizationId
            };
            await interviewTranscriptRepository.CreateInterviewTranscriptAsync(transcriptRequest, conversationText);
            _logger.LogInformation("Transcript processed and updated. InterviewUpdateId={InterviewUpdateId}", evaluationResult);
            _logger.LogInformation("HandleCallAnalyzedAsync END");

        }
    }
}
