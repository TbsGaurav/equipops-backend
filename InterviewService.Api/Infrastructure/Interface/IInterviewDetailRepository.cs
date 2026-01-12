using InterviewService.Api.ViewModels.Request.Interview_Detail;
using InterviewService.Api.ViewModels.Response.Interview_Detail;

namespace InterviewService.Api.Infrastructure.Interface
{
    public interface IInterviewDetailRepository
    {
        Task<InterviewCompleteResponseViewModel> InterviewDetailCreateAsync(InterviewDetailRequestViewModel model);
        Task<InterviewDetailResponseViewModel> InterviewDetailAsync(Guid? interviewid, Guid? Candidateid);
    }
}
