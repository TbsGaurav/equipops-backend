using InterviewService.Api.ViewModels.Request.Interviewer_setting;
using InterviewService.Api.ViewModels.Response.Interviewer_setting;

namespace InterviewService.Api.Infrastructure.Interface
{
    public interface IInterviewerSettingRepository
    {
        Task<InterviewerSettingListResponseViewModel> GetInterviewerSettingsAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null);
        Task<InterviewerSettingData> GetInterviewerSettingByIdAsync(Guid? Id);
        Task<InterviewerSettingCreateUpdateResponseViewModel> CreateUpdateInterviewerSettingAsync(InterviewerSettingCreateUpdateRequestViewModel request);
        Task DeleteInterviewerSettingAsync(InterviewerSettingDeleteRequestViewModel request);
    }
}
