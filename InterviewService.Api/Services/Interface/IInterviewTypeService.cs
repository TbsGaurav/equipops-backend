using Common.Services.ViewModels.General;

using InterviewService.Api.Helpers.ResponseHelpers.Models;
using InterviewService.Api.ViewModels.Request.Interview_Type;
using InterviewService.Api.ViewModels.Response.Interview;

namespace InterviewService.Api.Services.Interface
{
    public interface IInterviewTypeService
    {
        Task<ApiResponse<InterviewTypeListResponseViewModel>> GetInterviewTypesAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null);
        Task<ApiResponse<InterviewTypeByIdResponseViewModel>> GetInterviewTypeByIdAsync(Guid Id);
        Task<ApiResponse<InterviewTypeCreateUpdateResponseViewModel>> CreateUpdateInterviewTypeAsync(InterviewTypeCreateUpdateRequestViewModel model);
        Task<ApiResponse<InterviewTypeListResponseViewModel>> DeleteInterviewTypeAsync(InterviewTypeDeleteRequestViewModel model);
        Task<ApiResponse<JobObjectiveResponse>> GetJobObjectiveAsync(string jobType, string workMode, int experienceYears, string objective);
    }
}
