using Common.Services.Services.Interface;
using Common.Services.ViewModels.InterviewEvaluation;
using Common.Services.ViewModels.RetellAI;

using InterviewService.Api.Helpers.ResponseHelpers.Enums;
using InterviewService.Api.Helpers.ResponseHelpers.Handlers;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interview_transcript;
using InterviewService.Api.ViewModels.Request.InterviewUpdate;
using InterviewService.Api.ViewModels.Response.Interview_transcript;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.Text;

namespace InterviewService.Api.Services.Implementation
{
    public class InterviewTranscriptService(ILogger<InterviewTranscriptService> logger, IInterviewerRepository interviewerRepository, RetellAIEndpoints retellAIEndpoints, IInterviewTranscriptRepository interviewTranscriptRepository, IInterviewEvaluationService interviewEvaluationService) : IInterviewTranscriptService
    {
        public async Task<IActionResult> CreateInterviewTranscriptAsync(InterviewTrasncriptCreateRequestViewModel request)
        {
            logger.LogInformation("InterviewerService: CreateInterviewTranscriptAsync START. call_id={call_id}", request.Call_id);

            // 🔹 Validation
            if (string.IsNullOrWhiteSpace(request.Call_id))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Call ID is required.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            if (request.Interview_id == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<Guid>.Error(
                        "Interview Id is required.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            if (request.Organization_id == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<Guid>.Error(
                        "Organization Id is required.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }
            var retellKey = await interviewerRepository.GetRetellAiKey(request.Organization_id);

            if (string.IsNullOrEmpty(retellKey))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "RetellAI API Key is not configured for this organization.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            if (request.Interview_id == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<Guid>.Error(
                        "Interview Id is required.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            var openAiKey = await interviewerRepository.GetOpenAiKey(request.Organization_id);

            if (string.IsNullOrEmpty(openAiKey))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Open API Key is not configured for this organization.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            try
            {
                using var http = new HttpClient();
                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", retellKey);

                //call retell api to get transcript by id
                var url = retellAIEndpoints.GetCallById(request.Call_id);
                var response = await http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError(
                        "Retell AI get-call failed. StatusCode={StatusCode}",
                        response.StatusCode
                    );

                    return new ObjectResult(
                        ResponseHelper<string>.Error(
                            "Failed to fetch interview transcript.",
                            statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                        )
                    )
                    { StatusCode = StatusCodes.Status500InternalServerError };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var callResponse = JsonConvert
                    .DeserializeObject<RetellGetCallResponse>(jsonResponse);
                var conversationTextBuilder = new StringBuilder();

                if (callResponse?.TranscriptObject != null && callResponse.TranscriptObject.Any())
                {
                    logger.LogInformation("Interview transcript process started. InterviewId={InterviewId}", request.Interview_id);

                    var (interviewUpdateId, evaluationResult) = await ProcessAndUpdateInterviewTranscriptAsync(callResponse, request, openAiKey);

                    logger.LogInformation("Interview transcript process completed. InterviewId={InterviewId}", request.Interview_id);

                    foreach (var item in callResponse.TranscriptObject)
                    {
                        bool isInterviewer = item.Role?.ToLower() == "agent";

                        var transcript = new InterviewTrasncriptCreateRequestViewModel
                        {
                            interview_update_id = !string.IsNullOrWhiteSpace(interviewUpdateId) ? Guid.Parse(interviewUpdateId) : null,
                            Interview_id = request.Interview_id,
                            Interviewer_id = isInterviewer ? request.Interviewer_id : (Guid?)null,
                            Candidate_id = isInterviewer ? (Guid?)null : request.Candidate_id,
                            Call_id = request.Call_id,
                        };

                        var conversationText = item.Content;

                        await interviewTranscriptRepository.CreateInterviewTranscriptAsync(transcript, conversationText);
                    }

                    return new OkObjectResult(ResponseHelper<InterviewEvaluationResponse>.Success("Call ended successfully!", evaluationResult));
                }

                logger.LogInformation(
                    "InterviewerService: CreateInterviewTranscriptAsync SUCCESS. call_id={call_id}",
                    request.Call_id
                );

                return new OkObjectResult(ResponseHelper<string>.Success("Failed to fetch interview transcript."));
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "InterviewerService: CreateInterviewTranscriptAsync ERROR. call_id={call_id}",
                    request.Call_id
                );

                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Unexpected error while fetching transcript.",
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                )
                { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        public async Task<(string InterviewUpdateId, InterviewEvaluationResponse EvaluationResult)> ProcessAndUpdateInterviewTranscriptAsync(RetellGetCallResponse callResponse, InterviewTrasncriptCreateRequestViewModel request, string openAiKey)
        {
            // 🔹 Build transcript text
            var transcriptText = BuildTranscriptFromList(callResponse.TranscriptObject);

            // 🔹 Prepare transcript evaluation request
            var transcriptRequest = new InterviewTranscriptRequest
            {
                Transcript = transcriptText,
                key = openAiKey
            };

            // 🔹 Call evaluation service
            var interviewTranscriptResult =
                await interviewEvaluationService.EvaluateAsync(transcriptRequest);

            // 🔹 Prepare update model
            var interviewUpdate = new TranscriptInterviewUpdateRequestViewModel
            {
                candidate_id = request.Candidate_id ?? Guid.NewGuid(),
                interview_id = request.Interview_id ?? Guid.NewGuid(),
                communication = Convert.ToDecimal(interviewTranscriptResult.OverallScores.Communication),
                confidence = Convert.ToDecimal(interviewTranscriptResult.OverallScores.Confidence),
                technical = Convert.ToDecimal(interviewTranscriptResult.OverallScores.Technical),
                overall = Convert.ToDecimal(interviewTranscriptResult.OverallScores.Overall),
                hiring_decision = interviewTranscriptResult.HiringDecision,
                hiring_reason = interviewTranscriptResult.HiringReason,
                transcript = transcriptText,
                created_by = request.Organization_id,
                questions_evaluation = JsonConvert.SerializeObject(interviewTranscriptResult.Evaluations)
            };

            // 🔹 Update DB and return ID
            var id = await interviewTranscriptRepository.UpdateInterview(interviewUpdate);

            return (id, interviewTranscriptResult);
        }


        private static string BuildTranscriptFromList(List<RetellTranscriptItem> items)
        {
            if (items == null || items.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var item in items)
            {
                sb.AppendLine($"{item.Role}: {item.Content}");
            }

            return sb.ToString();
        }
    }
}
