using OrganizationService.Api.Helpers.ResponseHelpers.Enums;
using OrganizationService.Api.ViewModels.Response.Dashboard;

namespace OrganizationService.Api.Infrastructure.Interface
{
    public interface IDashboardRepository
    {
        Task<DashboardSummaryResponse> GetTotalAsync();
        Task<List<DepartmentCountResponse>> GetDepartmentWiseAsync(DateTime filterDate);
        Task<List<ApplicationsTrendResponse>> GetApplicationsTrendAsync(DateTime startDate, DateTime endDate);
        Task<CurrentVacanciesDashboardResponse> GetCurrentVacanciesAsync();
        Task<List<TaskProgressResponse>> GetTaskProgressAsync();
        Task<List<TodayScheduleResponse>> GetTodayScheduleAsync();
        Task<List<ApplicantListResponse>> GetApplicantsAsync(ApplicationStatus? status, DateTime interviewDate);
        Task<DashboardOrgSummaryResponse> GetOrganizationSummaryAsync();
        Task<List<DashboardOrgMonthlySummaryResponse>> GetOrganizationSummaryByMonthAsync();
        Task<List<JobTypeReportResponse>> GetJobTypeWiseAsync(DateTime date);
        Task<List<WorkModeReportResponse>> GetWorkModeWiseAsync(DateTime date);
    }
}
