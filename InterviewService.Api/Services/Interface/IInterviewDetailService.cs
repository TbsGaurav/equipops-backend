using Common.Services.Helper;

using InterviewService.Api.ViewModels.Request.Interview_Detail;
using InterviewService.Api.ViewModels.Response.Interview_Detail;

namespace InterviewService.Api.Services.Interface
{
    public interface IInterviewDetailService
    {
        Task<ApiResponse<InterviewCompleteResponseViewModel>> InterviewDetailCreateAsync(InterviewDetailRequestViewModel model);
        Task<ApiResponse<InterviewDetailResponseViewModel>> InterviewDetailAsync(Guid? interview_id, Guid? candidate_id);
    }
}
