using Microsoft.AspNetCore.Mvc;

using OrganizationService.Api.Helpers.ResponseHelpers.Enums;
using OrganizationService.Api.Helpers.ResponseHelpers.Handlers;
using OrganizationService.Api.Services.Interface;
using OrganizationService.Api.ViewModels.Response.Dashboard;

namespace OrganizationService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _iDashboardService;
        private readonly ILogger<DashboardController> _iLogger;

        public DashboardController(IDashboardService iDashboardService, ILogger<DashboardController> iLogger)
        {
            _iDashboardService = iDashboardService;
            _iLogger = iLogger;
        }

        [HttpGet("dashboard-report")]
        public async Task<IActionResult> GetDashboardReport()
        {
            // 🔹 Service Call
            _iLogger.LogInformation("Calling DashboardService.GetDashboardReportAsync.");

            var result = await _iDashboardService.GetDashboardReportAsync();

            return Ok(
                ResponseHelper<DashboardReportResponse>.Success(
                    result.Message ?? "Dashboard report retrieved successfully.",
                    result.Data
                )
            );
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            // 🔹 Service Call
            _iLogger.LogInformation("Calling DashboardService.GetSummaryAsync.");

            var result = await _iDashboardService.GetSummaryAsync();
            return Ok(ResponseHelper<DashboardSummaryResponse>.Success(result.Message ?? "Summary retrieved successfully.", result.Data));
        }

        [HttpGet("department-stats")]
        public async Task<IActionResult> GetDepartmentStats([FromQuery] DateTime? date)
        {
            _iLogger.LogInformation("Calling DashboardService.GetDepartmentStatsAsync with date={Date}", date);

            var result = await _iDashboardService.GetDepartmentStatsAsync(date);

            return Ok(ResponseHelper<List<DepartmentCountResponse>>.Success(result.Message ?? "Department stats retrieved successfully.",
               result.Data?.ToList() ?? []));
        }

        [HttpGet("applications-trend")]
        public async Task<IActionResult> GetApplicationsTrend([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            _iLogger.LogInformation(
                "Calling GetApplicationsTrend with startDate={StartDate}, endDate={EndDate}",
                startDate, endDate
            );

            var result = await _iDashboardService.GetApplicationsTrendAsync(startDate, endDate);

            return Ok(ResponseHelper<List<ApplicationsTrendResponse>>.Success(
                "Applications trend retrieved successfully.",
                result.Data?.ToList() ?? []
            ));
        }

        [HttpGet("current-vacancies")]
        public async Task<IActionResult> GetCurrentVacancies()
        {
            _iLogger.LogInformation("Calling GetCurrentVacancies");

            var result = await _iDashboardService.GetCurrentVacanciesAsync();

            return Ok(ResponseHelper<CurrentVacanciesDashboardResponse>.Success(
                "Current vacancies retrieved successfully.",
                result.Data ?? new CurrentVacanciesDashboardResponse()
            ));
        }

        [HttpGet("task-progress")]
        public async Task<IActionResult> GetTaskProgress()
        {
            _iLogger.LogInformation("Calling GetTaskProgress");

            var result = await _iDashboardService.GetTaskProgressAsync();

            return Ok(ResponseHelper<List<TaskProgressResponse>>.Success(
                "Task progress retrieved successfully.",
                result.Data ?? []
            ));
        }

        [HttpGet("today-schedule")]
        public async Task<IActionResult> GetTodaySchedule()
        {
            _iLogger.LogInformation("Calling GetTodaySchedule");

            var result = await _iDashboardService.GetTodayScheduleAsync();

            return Ok(ResponseHelper<List<TodayScheduleResponse>>.Success(
                "Today's schedule retrieved successfully.",
                result.Data ?? []
            ));
        }

        [HttpGet("applicants")]
        public async Task<IActionResult> GetApplicants([FromQuery] ApplicationStatus? status, [FromQuery] DateTime? interviewDate)
        {
            _iLogger.LogInformation("GetApplicants called");

            var result = await _iDashboardService.GetApplicantsAsync(status, interviewDate);

            return Ok(ResponseHelper<List<ApplicantListResponse>>.Success(
                "Applicants retrieved successfully",
                result.Data ?? []
            ));
        }

        [HttpGet("organization-summary")]
        public async Task<IActionResult> GetOrganizationSummary()
        {
            _iLogger.LogInformation("API Hit: GetOrganizationSummary");

            var result = await _iDashboardService.GetOrganizationAllSummaryAsync();
            return Ok(result);
        }

        [HttpGet("jobtype-stats")]
        public async Task<IActionResult> GetJobTypeStats([FromQuery] DateTime date)
        {
            _iLogger.LogInformation("Calling DashboardService.GetJobTypeStats with date={Date}", date);

            var result = await _iDashboardService.GetJobTypeWiseAsync(date);

            return Ok(ResponseHelper<List<JobTypeReportResponse>>.Success(result.Message ?? "Job types retrieved successfully.",
               result.Data?.ToList() ?? []));
        }

        [HttpGet("workmode-stats")]
        public async Task<IActionResult> GetWorkModeWiseAsync([FromQuery] DateTime date)
        {
            _iLogger.LogInformation("Calling DashboardService.GetJobTypeStats with date={Date}", date);

            var result = await _iDashboardService.GetWorkModeWiseAsync(date);

            return Ok(ResponseHelper<List<WorkModeReportResponse>>.Success(result.Message ?? "Work modes retrieved successfully.",
               result.Data?.ToList() ?? []));
        }
    }
}
