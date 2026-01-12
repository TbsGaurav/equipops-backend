using Common.Services.Helper;

using OrganizationService.Api.Helpers.ResponseHelpers.Enums;
using OrganizationService.Api.ViewModels.Response.Dashboard;

namespace OrganizationService.Api.Services.Interface
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardSummaryResponse>> GetSummaryAsync();
        Task<ApiResponse<List<DepartmentCountResponse>>> GetDepartmentStatsAsync(DateTime? date);
        Task<ApiResponse<List<ApplicationsTrendResponse>>> GetApplicationsTrendAsync(DateTime? startDate, DateTime? endDate);
        Task<ApiResponse<CurrentVacanciesDashboardResponse>> GetCurrentVacanciesAsync();
        Task<ApiResponse<List<TaskProgressResponse>>> GetTaskProgressAsync();
        Task<ApiResponse<List<TodayScheduleResponse>>> GetTodayScheduleAsync();
        Task<ApiResponse<List<ApplicantListResponse>>> GetApplicantsAsync(ApplicationStatus? status, DateTime? interviewDate);
        Task<ApiResponse<DashboardReportResponse>> GetDashboardReportAsync();
        Task<ApiResponse<DashboardOrgReportResponse>> GetOrganizationSummaryAsync();
        Task<ApiResponse<DashboardOrgReportResponse>> GetOrganizationSummaryByMonthAsync();
        Task<ApiResponse<DashboardOrgReportResponse>> GetOrganizationAllSummaryAsync();
        Task<ApiResponse<List<WorkModeReportResponse>>> GetWorkModeWiseAsync(DateTime date);
        Task<ApiResponse<List<JobTypeReportResponse>>> GetJobTypeWiseAsync(DateTime date);
    }
}
