using InterviewService.Api.Helpers.ResponseHelpers.Models;
using InterviewService.Api.ViewModels.Request.Interviewer_setting;
using InterviewService.Api.ViewModels.Response.Interviewer_setting;

namespace InterviewService.Api.Services.Interface
{
    public interface IInterviewerSettingService
    {
        Task<ApiResponse<InterviewerSettingListResponseViewModel>> GetInterviewerSettingsAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null);
        Task<ApiResponse<InterviewerSettingData>> GetInterviewerSettingByIdAsync(Guid Id);
        Task<ApiResponse<InterviewerSettingCreateUpdateResponseViewModel>> CreateUpdateInterviewerSettingAsync(InterviewerSettingCreateUpdateRequestViewModel model);
        Task<ApiResponse<InterviewerSettingListResponseViewModel>> DeleteInterviewerSettingAsync(InterviewerSettingDeleteRequestViewModel model);
    }
}
