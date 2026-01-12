using Common.Services.Helper;

using InterviewService.Api.ViewModels.Request.Interview_Form;
using InterviewService.Api.ViewModels.Response.Interview_Form;

namespace InterviewService.Api.Services.Interface
{
    public interface IInterviewFormService
    {
        #region Interview Form
        Task<ApiResponse<InterviewFormCreateUpdateResponseViewModel>> InterviewFormCreateAsync(InterviewFormRequestViewModel model);
        Task<ApiResponse<InterviewFormListResponseViewModel>> InterviewFormListAsync(string? search, bool? Is_Active, int length, int page, string orderColumn, string orderDirection);
        Task<ApiResponse<InterviewFormDeleteResponseViewModel>> InterviewFormDeleteAsync(InterviewFormDeleteRequestViewModel model);
        Task<ApiResponse<InterviewFormResponseViewModel>> InterviewFormByIdAsync(Guid? id);
        #endregion
    }
}
