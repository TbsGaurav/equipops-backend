using InterviewService.Api.ViewModels.Request.Webhook;
using InterviewService.Api.ViewModels.Response.Interview_Detail;
using System.Text.Json;

namespace InterviewService.Api.Infrastructure.Interface
{
    public interface IWebHookRepository
    {
        Task HandleCallStartedAsync(string callId);
        Task HandleCallEndedAsync(string callId, JsonElement body);
        Task<CandidateInterviewInvitationViewModel?> GetCandidateInterviewByCallIdAsync(string callId);
        //Task<string?> GetOpenAiKey(Guid organizationId, Guid interviewId);
    }
}
