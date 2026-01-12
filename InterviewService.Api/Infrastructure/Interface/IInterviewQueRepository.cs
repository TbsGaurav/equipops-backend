using InterviewService.Api.ViewModels.Request.Interview_Que;
using InterviewService.Api.ViewModels.Response.Interview_Que;

namespace InterviewService.Api.Infrastructure.Interface
{
    public interface IInterviewQueRepository
    {
        #region Interview Que
        Task<InterviewQueCreateUpdateResponseViewModel> InterviewQueCreateAsync(InterviewQueRequestViewModel request);
        Task<InterviewQueListResponseViewModel> InterviewQueListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection);
        Task<InterviewQueDeleteResponseViewModel> InterviewQueDeleteAsync(InterviewQueDeleteRequestViewModel request);
        Task<InterviewQueResponseViewModel> InterviewQueByIdAsync(Guid? id);
        Task<List<InterviewQuestionBulkDto>> GetInterviewQuestionsByInterviewIdAsync(Guid interviewId);
        #endregion
    }
}
