using Common.Services.Helper;

using OrganizationService.Api.Helpers.ResponseHelpers.Enums;
using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.Services.Interface;
using OrganizationService.Api.ViewModels.Response.Dashboard;

namespace OrganizationService.Api.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _iDashboardRepository;
        private readonly ILogger<DashboardService> _logger;
        public DashboardService(IDashboardRepository iDashboardRepository, ILogger<DashboardService> logger)
        {
            _iDashboardRepository = iDashboardRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<DashboardSummaryResponse>> GetSummaryAsync()
        {
            // 🔹 Repository Call
            _logger.LogInformation("Calling DashboardRepository.GetSummaryAsync.");

            var data = await _iDashboardRepository.GetTotalAsync();

            // 🔹 Success
            _logger.LogInformation("Summary retrieved successfully.");

            return new ApiResponse<DashboardSummaryResponse>
            {
                Success = true,
                Message = "Summary retrieved successfully.",
                Data = data
            };
        }
        public async Task<ApiResponse<List<DepartmentCountResponse>>> GetDepartmentStatsAsync(DateTime? date)
        {
            _logger.LogInformation("GetDepartmentStatsAsync started");

            // ✅ Default to today if null
            var filterDate = (date ?? DateTime.UtcNow).Date;

            var data = await _iDashboardRepository.GetDepartmentWiseAsync(filterDate);

            return new ApiResponse<List<DepartmentCountResponse>>
            {
                Success = true,
                Message = "Department stats retrieved successfully.",
                Data = data
            };
        }
        public async Task<ApiResponse<List<ApplicationsTrendResponse>>> GetApplicationsTrendAsync(DateTime? startDate, DateTime? endDate)
        {
            _logger.LogInformation("GetApplicationsTrendAsync started");

            var today = DateTime.UtcNow.Date;

            var fromDate = startDate?.Date ?? today;
            var toDate = endDate?.Date ?? fromDate.AddDays(7);

            if (fromDate > toDate)
                throw new ArgumentException("Start date cannot be greater than end date");

            var data = await _iDashboardRepository.GetApplicationsTrendAsync(fromDate, toDate);

            return new ApiResponse<List<ApplicationsTrendResponse>>
            {
                Success = true,
                Message = "Applications trend retrieved successfully.",
                Data = data
            };
        }
        public async Task<ApiResponse<CurrentVacanciesDashboardResponse>> GetCurrentVacanciesAsync()
        {
            _logger.LogInformation("GetCurrentVacanciesAsync started");

            var data = await _iDashboardRepository.GetCurrentVacanciesAsync();

            return new ApiResponse<CurrentVacanciesDashboardResponse>
            {
                Success = true,
                Message = "Current vacancy retrieved successfully.",
                Data = data
            };
        }
        public async Task<ApiResponse<List<TaskProgressResponse>>> GetTaskProgressAsync()
        {
            _logger.LogInformation("GetTaskProgressAsync started");

            var data = await _iDashboardRepository.GetTaskProgressAsync();

            return new ApiResponse<List<TaskProgressResponse>>
            {
                Success = true,
                Message = "Task progress retrieved successfully.",
                Data = data
            };
        }
        public async Task<ApiResponse<List<TodayScheduleResponse>>> GetTodayScheduleAsync()
        {
            _logger.LogInformation("GetTodayScheduleAsync started");

            var data = await _iDashboardRepository.GetTodayScheduleAsync();

            return new ApiResponse<List<TodayScheduleResponse>>
            {
                Success = true,
                Message = "Today schedule retrieved successfully.",
                Data = data
            };
        }
        public async Task<ApiResponse<List<ApplicantListResponse>>> GetApplicantsAsync(ApplicationStatus? status, DateTime? interviewDate)
        {
            _logger.LogInformation("GetApplicantsAsync started");

            var filterDate = interviewDate ?? DateTime.UtcNow.Date;

            var data = await _iDashboardRepository.GetApplicantsAsync(status, filterDate);

            return new ApiResponse<List<ApplicantListResponse>>
            {
                Success = true,
                Message = "Applicants list retrieved successfully.",
                Data = data
            };
        }
        public async Task<ApiResponse<DashboardReportResponse>> GetDashboardReportAsync()
        {
            _logger.LogInformation("DashboardService.GetDashboardReportAsync started");

            try
            {
                var today = DateTime.UtcNow.Date;
                var startDate = today;
                var endDate = today.AddDays(7);

                _logger.LogInformation("Fetching dashboard summary");
                var summary = await _iDashboardRepository.GetTotalAsync();

                _logger.LogInformation("Fetching applications by department for date={Date}", today);
                var departments = await _iDashboardRepository.GetDepartmentWiseAsync(today);

                _logger.LogInformation("Fetching application trends from {StartDate} to {EndDate}", startDate, endDate);
                var applicationsTrends = await _iDashboardRepository.GetApplicationsTrendAsync(startDate, endDate);
                
                _logger.LogInformation("Fetching job type");
                var jobTypes = await _iDashboardRepository.GetJobTypeWiseAsync(today);

                _logger.LogInformation("Fetching work mode type");
                var workModes = await _iDashboardRepository.GetWorkModeWiseAsync(today);

                _logger.LogInformation("Fetching current vacancies");
                var currentVacancies = await _iDashboardRepository.GetCurrentVacanciesAsync();

                _logger.LogInformation("Fetching today schedule for date={Date}", today);
                var todaySchedule = await _iDashboardRepository.GetTodayScheduleAsync();

                _logger.LogInformation("Fetching applicants list");
                var applicants = await _iDashboardRepository.GetApplicantsAsync(ApplicationStatus.All, today);

                _logger.LogInformation("Dashboard data assembled successfully");

                return new ApiResponse<DashboardReportResponse>
                {
                    Success = true,
                    Message = "Dashboard reports retrieved successfully.",
                    Data = new DashboardReportResponse
                    {
                        Summary = summary,
                        ApplicationsByDepartment = departments,
                        ApplicationTrends = applicationsTrends,
                        CurrentVacancies = currentVacancies,
                        TodaySchedule = todaySchedule,
                        Applicants = applicants,
                        jobTypeReports = jobTypes,
                        workModeReports = workModes
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating dashboard report");
                throw;
            }
        }
        public async Task<ApiResponse<DashboardOrgReportResponse>> GetOrganizationAllSummaryAsync()
        {
            _logger.LogInformation("Service: GetOrganizationAllSummaryAsync started");

            var summary = await _iDashboardRepository.GetOrganizationSummaryAsync();
            var summaryMonthly = await _iDashboardRepository.GetOrganizationSummaryByMonthAsync();

            return new ApiResponse<DashboardOrgReportResponse>
            {
                Success = true,
                Message = "Organization summary retrieved successfully.",
                Data = new DashboardOrgReportResponse
                {
                    dashboardOrgSummary = summary,
                    dashboardOrgMonthlySummary = summaryMonthly
                }
            };
        }
        public async Task<ApiResponse<DashboardOrgReportResponse>> GetOrganizationSummaryAsync()
        {
            _logger.LogInformation("Service: GetOrganizationSummaryAsync started");

            var summary = await _iDashboardRepository.GetOrganizationSummaryAsync();

            return new ApiResponse<DashboardOrgReportResponse>
            {
                Success = true,
                Message = "Organization summary retrieved successfully.",
                Data = new DashboardOrgReportResponse
                {
                    dashboardOrgSummary = summary
                }
            };
        }
        public async Task<ApiResponse<DashboardOrgReportResponse>> GetOrganizationSummaryByMonthAsync()
        {
            _logger.LogInformation("Service: GetOrganizationSummaryByMonthAsync started");

            var summaryMonthly = await _iDashboardRepository.GetOrganizationSummaryByMonthAsync();

            return new ApiResponse<DashboardOrgReportResponse>
            {
                Success = true,
                Message = "Organization summary retrieved successfully.",
                Data = new DashboardOrgReportResponse
                {
                    dashboardOrgMonthlySummary = summaryMonthly
                }
            };
        }

        public async Task<ApiResponse<List<JobTypeReportResponse>>> GetJobTypeWiseAsync(DateTime date)
        {
            _logger.LogInformation("GetJobTypeWiseAsync started");

            var data = await _iDashboardRepository.GetJobTypeWiseAsync(date);

            return new ApiResponse<List<JobTypeReportResponse>>
            {
                Success = true,
                Message = "Job types retrieved successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<List<WorkModeReportResponse>>> GetWorkModeWiseAsync(DateTime date)
        {
            _logger.LogInformation("GetWorkModeWiseAsync started");

            var data = await _iDashboardRepository.GetWorkModeWiseAsync(date);

            return new ApiResponse<List<WorkModeReportResponse>>
            {
                Success = true,
                Message = "Work mode retrieved successfully.",
                Data = data
            };
        }
    }
}
