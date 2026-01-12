using Common.Services.ViewModels.InterviewEvaluation;
using InterviewService.Api.ViewModels.Request.Interview_transcript;
using InterviewService.Api.ViewModels.Response.Interview_transcript;
using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Api.Services.Interface
{
    public interface IInterviewTranscriptService
    {
        Task<IActionResult> CreateInterviewTranscriptAsync(InterviewTrasncriptCreateRequestViewModel request);
        Task<(string InterviewUpdateId, InterviewEvaluationResponse EvaluationResult)> ProcessAndUpdateInterviewTranscriptAsync(RetellGetCallResponse callResponse, InterviewTrasncriptCreateRequestViewModel request, string openAiKey);
    }
}
