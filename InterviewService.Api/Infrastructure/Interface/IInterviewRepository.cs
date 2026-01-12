using InterviewService.Api.ViewModels.Request.Interview;
using InterviewService.Api.ViewModels.Response.Interview;
using InterviewService.Api.ViewModels.Response.Interview_Detail;

namespace InterviewService.Api.Infrastructure.Interface
{
    public interface IInterviewRepository
    {
        Task<InterviewListResponseViewModel> GetInterviewsAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null, Guid? OrganizationId = null);
        Task<InterviewByIdResponseViewModel> GetInterviewByIdAsync(Guid Id);
        Task<InterviewInitResponseViewModel> GetInterviewInitAsync();
        Task<InterviewCreateResponseViewModel> CreateInterviewAsync(InterviewCreateRequestViewModel request, string documentFile);
        Task<InterviewUpdateResponseViewModel> UpdateInterviewAsync(InterviewUpdateRequestViewModel request, string documentFile);
        Task DeleteInterviewAsync(InterviewDeleteRequestViewModel request);
        Task<string> CreateInterviewTokenAsync(InterviewTokenRequestViewModel request);
        Task<(int StatusCode, string Message, string? TokenData)> VerifyInterviewTokenAsync(string token);
        Task<string?> GetCandidateEmailByIdAsync(Guid candidateId);
        Task<InterviewOverviewResponseViewModel> GetInterviewOverviewAsync(Guid interviewId, Guid candidateId, Guid interviewerId);
        Task<CandidateInterviewInvitationViewModel?> GetCandidateInterviewInvitationAsync(Guid id);
        Task<int> UpdateCallIdAsync(Guid candidateInterviewInvitationId, string callId);
    }
}
