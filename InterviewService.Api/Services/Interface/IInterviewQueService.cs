//using InterviewService.Api.Helpers.ResponseHelpers.Models;
using Common.Services.Helper;

using InterviewService.Api.ViewModels.Request.Interview_Que;
using InterviewService.Api.ViewModels.Response.Interview_Que;

namespace InterviewService.Api.Services.Interface
{
    public interface IInterviewQueService
    {
        #region Interview Que
        Task<ApiResponse<InterviewQueCreateUpdateResponseViewModel>> InterviewQueCreateAsync(InterviewQueRequestViewModel model);
        Task<ApiResponse<InterviewQueListResponseViewModel>> InterviewQueListAsync(string? search, bool? Is_Active, int length, int page, string orderColumn, string orderDirection);
        Task<ApiResponse<InterviewQueDeleteResponseViewModel>> InterviewQueDeleteAsync(InterviewQueDeleteRequestViewModel model);
        Task<ApiResponse<InterviewQueResponseViewModel>> InterviewQueByIdAsync(Guid? id);
        #endregion
    }
}
