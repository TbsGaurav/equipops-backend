using Dapper;

using OrganizationService.Api.Helpers.ResponseHelpers.Enums;
using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.ViewModels.Response.Dashboard;

using System.Data;
using System.Globalization;

namespace OrganizationService.Api.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ILogger<DashboardRepository> _logger;
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        public DashboardRepository(IDbConnectionFactory dbFactory, ILogger<DashboardRepository> logger, IHttpContextAccessor contextAccessor)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        public async Task<DashboardSummaryResponse> GetTotalAsync()
        {
            _logger.LogInformation("GetTotalAsync started");

            var user = _contextAccessor.HttpContext?.User;
            if (user == null)
                throw new UnauthorizedAccessException("User not logged in");

            var orgIdClaim = user.FindFirst("organization_id")?.Value;

            if (!Guid.TryParse(orgIdClaim, out Guid organizationId))
                throw new UnauthorizedAccessException("Invalid organization id");

            const string sql = @"
                                SELECT
                                    COUNT(u.id) AS total_applications,
                                    COUNT(*) FILTER (WHERE u.hiring_decision = 'Shortlisted') AS shortlisted,
                                    COUNT(*) FILTER (WHERE u.hiring_decision = 'Hired')       AS hired,
                                    COUNT(*) FILTER (WHERE u.hiring_decision = 'Reject')    AS rejected
                                FROM interviews.interviews i
                                INNER JOIN interviews.interview_update u
                                       ON u.interview_id = i.id
                                      AND u.is_delete = false
                                WHERE i.organization_id = @OrganizationId
                                  AND i.is_delete = false;
                            ";

            try
            {
                using var conn = _dbFactory.CreateConnection();
                conn.Open();

                var result = await conn.QuerySingleAsync<DashboardSummaryResponse>(
                    sql,
                    new { OrganizationId = organizationId }
                );

                _logger.LogInformation(
                    "Interview dashboard counts fetched successfully for OrganizationId={OrganizationId}",
                    organizationId
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching interview dashboard counts");
                throw;
            }
        }

        public async Task<List<DepartmentCountResponse>> GetDepartmentWiseAsync(DateTime date)
        {
            _logger.LogInformation("GetDepartmentWiseAsync started with date={Date}", date);

            var user = _contextAccessor.HttpContext?.User;
            if (user == null)
                throw new UnauthorizedAccessException("User not logged in");

            var orgIdClaim = user.FindFirst("organization_id")?.Value;

            if (!Guid.TryParse(orgIdClaim, out Guid organizationId))
                throw new UnauthorizedAccessException("Invalid organization id");

            const string sql = @"
                                SELECT
                                    d.name AS department,
                                    COUNT(i.id) AS total_count
                                FROM interviews.interviews i
                                INNER JOIN master.industry_type_department d
                                    ON d.id = i.department_id
                                WHERE i.organization_id = @OrganizationId
                                  AND i.is_delete = false
                                  AND d.is_delete = false
                                  AND i.created_date::date = @FilterDate
                                GROUP BY d.name
                                ORDER BY total_count DESC;
                            ";

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            var result = await conn.QueryAsync<DepartmentCountResponse>(
                sql,
                new
                {
                    OrganizationId = organizationId,
                    FilterDate = date.Date
                }
            );

            return result.ToList();
        }


        public async Task<List<ApplicationsTrendResponse>> GetApplicationsTrendAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogInformation("GetApplicationsTrendAsync started with startDate={StartDate}, endDate={EndDate}", startDate, endDate);

            var user = _contextAccessor.HttpContext?.User
                ?? throw new UnauthorizedAccessException("User not logged in");

            var orgIdClaim = user.FindFirst("organization_id")?.Value;

            if (!Guid.TryParse(orgIdClaim, out Guid organizationId))
                throw new UnauthorizedAccessException("Invalid organization id");

            const string sql = @"SELECT
                                u.created_date::date::timestamp AS report_date,
                                COUNT(*) AS applied,
                                COUNT(*) FILTER (WHERE u.hiring_decision = 'Reject') AS shortlisted
                            FROM interviews.interviews i
                            INNER JOIN interviews.interview_update u
                                ON u.interview_id = i.id
                                AND u.is_delete = FALSE
                            WHERE i.organization_id = @OrganizationId
                              AND i.is_delete = FALSE
                              AND u.created_date::date BETWEEN @StartDate AND @EndDate
                            GROUP BY report_date
                            ORDER BY report_date;
                            ";

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            var raw = await conn.QueryAsync<ApplicationsTrendRaw>(
                    sql,
                    new
                    {
                        OrganizationId = organizationId,
                        StartDate = startDate.Date,
                        EndDate = endDate.Date
                    }
                );

            var result = raw.Select(x => new ApplicationsTrendResponse
            {
                ReportDate = x.report_date.ToString("dd MMM", CultureInfo.InvariantCulture),
                Applied = x.applied,
                Shortlisted = x.shortlisted
            }).ToList();

            return result.ToList();
        }

        public async Task<CurrentVacanciesDashboardResponse> GetCurrentVacanciesAsync()
        {
            _logger.LogInformation("GetCurrentVacanciesAsync started");

            var user = _contextAccessor.HttpContext?.User
                ?? throw new UnauthorizedAccessException("User not logged in");

            var orgIdClaim = user.FindFirst("organization_id")?.Value;

            if (!Guid.TryParse(orgIdClaim, out Guid organizationId))
                throw new UnauthorizedAccessException("Invalid organization id");

            const string sql = @"SELECT
                                i.id AS interview_id,
                                i.name AS job_title,
                                it.interview_type AS job_type,
                                COUNT(cd.id) AS applicant_count
                            FROM interviews.interviews i
                            LEFT JOIN master.candidate_detail cd
                                ON cd.interview_id = i.id
                               AND cd.is_delete = FALSE
                            LEFT JOIN interviews.interview_type it
                                ON it.id = i.interview_type_id
                            WHERE i.organization_id = @OrganizationId
                              AND i.is_delete = FALSE
                              AND i.is_active = TRUE
                              AND i.job_status = TRUE
                            GROUP BY
                                i.id,
                                i.name,
                                it.interview_type
                            ORDER BY applicant_count DESC;";

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            var data = (await conn.QueryAsync<CurrentVacancyRaw>(
                        sql,
                        new { OrganizationId = organizationId }
                    )).ToList();

            var currentVacancies = new CurrentVacanciesDashboardResponse
            {
                Vacancies = data,
                TotalVacancies = data.Sum(p => p.applicant_count)
            };

            return currentVacancies;
        }

        public async Task<List<TaskProgressResponse>> GetTaskProgressAsync()
        {
            _logger.LogInformation("GetTaskProgressAsync started");

            var user = _contextAccessor.HttpContext?.User
                ?? throw new UnauthorizedAccessException("User not logged in");

            var orgIdClaim = user.FindFirst("organization_id")?.Value;

            if (!Guid.TryParse(orgIdClaim, out Guid organizationId))
                throw new UnauthorizedAccessException("Invalid organization id");

            const string sql = @"
                                SELECT
                                    id              AS TaskId,
                                    task_name       AS TaskName,
                                    progress        AS Progress,
                                    task_date       AS TaskDate
                                FROM interview_tasks
                                WHERE organization_id = @OrganizationId
                                  AND is_active = true
                                  AND is_delete = false
                                ORDER BY task_date DESC;
                            ";

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            return (await conn.QueryAsync<TaskProgressResponse>(
                sql,
                new { OrganizationId = organizationId }
            )).ToList();
        }

        public async Task<List<TodayScheduleResponse>> GetTodayScheduleAsync()
        {
            _logger.LogInformation("GetTodayScheduleAsync started");

            var user = _contextAccessor.HttpContext?.User
                ?? throw new UnauthorizedAccessException("User not logged in");

            var orgIdClaim = user.FindFirst("organization_id")?.Value;

            if (!Guid.TryParse(orgIdClaim, out var organizationId))
                throw new UnauthorizedAccessException("Invalid organization id");

            const string sql = @"SELECT
                            i.id AS interview_id,
                            i.name AS job_title,
                            COUNT(cd.id) AS applicant_count,
                            i.created_date AS schedule_time
                        FROM interviews.interviews i
                        LEFT JOIN master.candidate_detail cd
                            ON cd.interview_id = i.id
                           AND cd.is_delete = FALSE
                        WHERE i.organization_id = @OrganizationId
                          AND i.is_delete = FALSE
                          AND i.is_active = TRUE
                          AND i.job_status = TRUE
                          AND i.created_date::date = CURRENT_DATE
                        GROUP BY
                            i.id,
                            i.name,
                            i.created_date
                        ORDER BY i.created_date;";

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            var raw = await conn.QueryAsync<TodayScheduleRaw>(
                sql,
                new { OrganizationId = organizationId }
            );

            return raw.Select(x => new TodayScheduleResponse
            {
                JobTitle = x.Job_Title,
                Applicants = $"{x.Applicant_Count} Applicants",
                Time = x.Schedule_Time.ToString("h:mm tt", CultureInfo.InvariantCulture)
            }).ToList();
        }


        public async Task<List<ApplicantListResponse>> GetApplicantsAsync(ApplicationStatus? status, DateTime interviewDate)
        {
            var user = _contextAccessor.HttpContext?.User
                ?? throw new UnauthorizedAccessException();

            var orgIdClaim = user.FindFirst("organization_id")?.Value;

            if (!Guid.TryParse(orgIdClaim, out Guid organizationId))
                throw new UnauthorizedAccessException();

            const string sql = @"SELECT
                                cd.name                  AS candidate_name,
                                it.interview_type        AS employment_type,
                                i.name                   AS role,
                                iu.created_date          AS interview_date,
                                iu.hiring_decision       AS status
                            FROM master.candidate_detail cd
                            INNER JOIN interviews.interview_update iu
                                ON iu.candidate_id = cd.id
                               AND iu.is_delete = FALSE
                            INNER JOIN interviews.interviews i
                                ON i.id = iu.interview_id
                            INNER JOIN interviews.interview_type it
                                ON it.id = i.interview_type_id
                            WHERE i.organization_id = @OrganizationId
                              AND cd.is_delete = FALSE
                              AND cd.is_active = TRUE
                            ORDER BY iu.created_date DESC;
                            ";

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            var raw = await conn.QueryAsync<ApplicantListRaw>(
                        sql,
                        new { OrganizationId = organizationId }
                      );

            return raw.Select(x => new ApplicantListResponse
            {
                Name = x.Candidate_Name,
                EmploymentType = x.Employment_Type,
                Role = x.Role,
                InterviewDate = x.Interview_Date.ToString("MMM dd, yyyy", CultureInfo.InvariantCulture),
                Status = x.Status
            }).ToList();
        }

        public async Task<DashboardOrgSummaryResponse> GetOrganizationSummaryAsync()
        {
            _logger.LogInformation("GetOrganizationSummaryAsync started");

            const string sql = @"
                            SELECT
                                COUNT(id)                                              AS total_org,
                                COUNT(*) FILTER (WHERE is_active = true AND is_delete = false)               AS active_org,
                                COUNT(*) FILTER (WHERE is_active = false AND is_delete = false)              AS inactive_org,
                                COUNT(*) FILTER (WHERE is_delete = true)               AS deleted_org,
                                COUNT(*) FILTER (
                                    WHERE created_date >= now() - interval '1 month' AND is_delete = false
                                )                                                       AS new_org
                            FROM master.organization;
                        ";
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            var summary = await conn.QuerySingleAsync<DashboardOrgSummaryResponse>(sql);

            return summary;
        }

        public async Task<List<DashboardOrgMonthlySummaryResponse>> GetOrganizationSummaryByMonthAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            try
            {
                // 1️⃣ Call the stored procedure (creates & opens cursor)
                await conn.ExecuteAsync(
                    "CALL master.sp_get_org_monthly_summary_last_12_months('org_month_cursor');",
                    commandType: CommandType.Text,
                    transaction: tran
                );

                // 2️⃣ Fetch data from cursor
                var result = (await conn.QueryAsync<DashboardOrgMonthlySummaryResponse>(
                    "FETCH ALL FROM org_month_cursor;",
                    commandType: CommandType.Text,
                    transaction: tran
                )).ToList();

                // 3️⃣ Commit transaction
                tran.Commit();

                return result;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<List<JobTypeReportResponse>> GetJobTypeWiseAsync(DateTime date)
        {
            _logger.LogInformation("GetJobTypeWiseAsync started with date={Date}", date);

            var user = _contextAccessor.HttpContext?.User;
            if (user == null)
                throw new UnauthorizedAccessException("User not logged in");

            var orgIdClaim = user.FindFirst("organization_id")?.Value;

            if (!Guid.TryParse(orgIdClaim, out Guid organizationId))
                throw new UnauthorizedAccessException("Invalid organization id");

            const string sql = @"
                                SELECT
                                    it.interview_type AS job_type,
                                    COUNT(i.id) AS total_count
                                FROM interviews.interviews i
                                INNER JOIN interviews.interview_type it
                                    ON it.id = i.interview_type_id
                                WHERE i.organization_id = @OrganizationId
                                  AND i.is_delete = false
                                  AND i.created_date::date = @FilterDate
                                GROUP BY it.interview_type
                                ORDER BY total_count DESC;
                            ";

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            var result = await conn.QueryAsync<JobTypeReportResponse>(
                sql,
                new
                {
                    OrganizationId = organizationId,
                    FilterDate = date.Date
                }
            );

            return result.ToList();
        }

        public async Task<List<WorkModeReportResponse>> GetWorkModeWiseAsync(DateTime date)
        {
            _logger.LogInformation("GetWorkModeWiseAsync started with date={Date}", date);

            var user = _contextAccessor.HttpContext?.User;
            if (user == null)
                throw new UnauthorizedAccessException("User not logged in");

            var orgIdClaim = user.FindFirst("organization_id")?.Value;

            if (!Guid.TryParse(orgIdClaim, out Guid organizationId))
                throw new UnauthorizedAccessException("Invalid organization id");

            const string sql = @"
                                SELECT
                                    wm.work_mode AS work_mode,
                                    COUNT(i.id) AS total_count
                                FROM interviews.interviews i
                                INNER JOIN interviews.work_mode wm
                                    ON wm.id = i.work_mode_id
                                WHERE i.organization_id = @OrganizationId
                                  AND i.is_delete = false
                                  AND i.created_date::date = @FilterDate
                                GROUP BY wm.work_mode
                                ORDER BY total_count DESC;
                            ";

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            var result = await conn.QueryAsync<WorkModeReportResponse>(
                sql,
                new
                {
                    OrganizationId = organizationId,
                    FilterDate = date.Date
                }
            );

            return result.ToList();
        }

    }
}
