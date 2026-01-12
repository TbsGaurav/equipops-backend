using InterviewService.Api.ViewModels.Request.Interview_Form;
using InterviewService.Api.ViewModels.Response.Interview_Form;

namespace InterviewService.Api.Infrastructure.Interface
{
    public interface IInterviewFormRepository
    {
        #region Interview Form
        Task<InterviewFormCreateUpdateResponseViewModel> InterviewFormCreateAsync(InterviewFormRequestViewModel request);
        Task<InterviewFormListResponseViewModel> InterviewFormListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection);
        Task<InterviewFormDeleteResponseViewModel> InterviewFormDeleteAsync(InterviewFormDeleteRequestViewModel request);
        Task<InterviewFormResponseViewModel> InterviewFormByIdAsync(Guid? id);
        #endregion
    }
}
