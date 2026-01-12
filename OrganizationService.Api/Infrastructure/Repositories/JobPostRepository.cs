using Dapper;

using Microsoft.AspNetCore.Mvc.Rendering;

using Npgsql;

using OrganizationService.Api.Helpers;
using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.ViewModels.Request.JobPost;
using OrganizationService.Api.ViewModels.Response.JobPost;

using System.Data;

namespace OrganizationService.Api.Infrastructure.Repositories
{
    public class JobPostRepository : IJobPostRepository
    {
        private readonly ILogger<JobPostRepository> _logger;
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IHttpContextAccessor _contextAccessor;

        public JobPostRepository(IDbConnectionFactory dbFactory, ILogger<JobPostRepository> logger, IHttpContextAccessor contextAccessor)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        #region Job Templates

        #region Get Job Template List
        public async Task<JobTemplateListResponse> GetJobTemplatesAsync(string? search, int length, int page, string orderColumn, string orderDirection = "Asc", bool? isActive = null)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetJobTemplateList}(" +
                "@p_search, @p_length, @p_page, @p_order_column, " +
                "@p_order_direction, @p_is_active, @c_total_numbers, @refcursor)",
                (NpgsqlConnection)conn,
                (NpgsqlTransaction)tran
            );

            // INPUTS
            cmd.Parameters.AddWithValue("p_search", (object?)search ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_length", length);
            cmd.Parameters.AddWithValue("p_page", page);
            cmd.Parameters.AddWithValue("p_order_column", orderColumn);
            cmd.Parameters.AddWithValue("p_order_direction", orderDirection);
            cmd.Parameters.AddWithValue("p_is_active", (object?)isActive ?? DBNull.Value);

            // OUTPUT: TOTAL COUNT
            var totalParam = new NpgsqlParameter("c_total_numbers", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = ParameterDirection.InputOutput,
                Value = 0
            };
            cmd.Parameters.Add(totalParam);

            // OUTPUT: CURSOR
            var cursorParam = new NpgsqlParameter("refcursor", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "job_template_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            // EXECUTE PROCEDURE
            await cmd.ExecuteNonQueryAsync();

            // FETCH CURSOR DATA
            var templates = (await conn.QueryAsync<JobTemplate>(
                "FETCH ALL IN \"job_template_cursor\"",
                transaction: tran
            )).ToList();

            tran.Commit();

            return new JobTemplateListResponse
            {
                TotalNumbers = (int)totalParam.Value,
                jobTemplates = templates
            };
        }
        #endregion

        #region Get Job Template By Id
        public async Task<JobTemplateByIdResponse> GetJobTemplateByIdAsync(Guid id)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetJobTemplateById}(@p_id, @ref)",
                (NpgsqlConnection)conn);

            cmd.Transaction = (NpgsqlTransaction)tran;

            // Input
            cmd.Parameters.AddWithValue("p_id", id);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor"
            };

            cmd.Parameters.Add(cursorParam);

            cmd.ExecuteNonQuery();

            var templates = conn.Query<JobTemplate>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();
            var template = templates.FirstOrDefault();

            tran.Commit();

            return new JobTemplateByIdResponse
            {
                Template = template
            };
        }
        #endregion

        #region Create / Update Job Template
        public async Task<JobTemplateCreateUpdateResponse> CreateUpdateJobTemplateAsync(JobTemplateCreateUpdateRequest request)
        {
            _logger.LogInformation("Executing JobTemplateCreateUpdateAsync for Template Title={Title}", request.Title);

            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst("user_id")?.Value;

            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_id", request.Id);
            parameters.Add("p_title", request.Title);
            parameters.Add("p_subtitle", request.SubTitle);
            parameters.Add("p_department", request.Department);
            parameters.Add("p_location", request.Location);
            parameters.Add("p_employment_type", Guid.Parse(request.EmploymentType));
            parameters.Add("p_experience_min", request.ExperienceMin);
            parameters.Add("p_experience_max", request.ExperienceMax);
            parameters.Add("p_salary_min", request.SalaryMin);
            parameters.Add("p_salary_max", request.SalaryMax);
            parameters.Add("p_skills", request.Skills);
            parameters.Add("p_responsibilities", request.Responsibilities?.ToArray(), DbType.Object);
            parameters.Add("p_requirements", request.Requirements?.ToArray(), DbType.Object);
            parameters.Add("p_benefits", request.Benefits);
            parameters.Add("p_notes", request.Notes);
            parameters.Add("p_created_by", createdBy);
            parameters.Add("p_is_active", request.IsActive);

            // Output
            parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            await conn.ExecuteAsync(StoreProcedure.JobTemplateCreateUpdate, parameters, commandType: CommandType.StoredProcedure);

            var outId = parameters.Get<Guid?>("o_id") ?? Guid.Empty;

            return new JobTemplateCreateUpdateResponse
            {
                Id = outId,
                Title = request.Title,
                SubTitle = request.SubTitle,
                Department = request.Department,
                Location = request.Location,
                EmploymentType = request.EmploymentType,
                Skills = request.Skills
            };
        }
        #endregion

        #region Delete Job Template
        public async Task DeleteJobTemplateAsync(JobTemplateDeleteRequest request)
        {
            _logger.LogInformation("Executing DeleteJobTemplateAsync for Id={Id}", request.Id);

            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst("user_id")?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            parameters.Add("p_id", request.Id);
            parameters.Add("p_updated_by", updatedBy);

            await conn.ExecuteAsync(
                StoreProcedure.JobTemplateDelete,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        #endregion

        public async Task<List<SelectListItem>> GetEmploymentTypeListAsync()
        {
            using var conn = _dbFactory.CreateConnection();

            _logger.LogInformation("Fetching Employment Type list");

            const string query = @"
                    SELECT 
                        it.id::text AS Value,
                        it.interview_type AS Text
                    FROM interviews.interview_type it
                    WHERE it.is_active = true AND it.is_delete = false
                    ORDER BY it.interview_type;
                ";

            var employmentTypes = (await conn.QueryAsync<SelectListItem>(query)).ToList();

            if (!employmentTypes.Any())
                _logger.LogWarning("No employment types found");
            else
                _logger.LogInformation("Fetched {Count} employment types", employmentTypes.Count);

            return employmentTypes;
        }
        #endregion

        #region Job Posts

        #region Get Job Post List
        public async Task<JobPostListResponse> GetJobPostsAsync(string? search, int length, int page, string orderColumn, string orderDirection = "Asc", bool? isActive = null)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetJobPostList}(" +
                "@p_search, @p_length, @p_page, @p_order_column, " +
                "@p_order_direction, @p_is_active, @c_total_numbers, @refcursor)",
                (NpgsqlConnection)conn,
                (NpgsqlTransaction)tran
            );

            // INPUTS
            cmd.Parameters.AddWithValue("p_search", (object?)search ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_length", length);
            cmd.Parameters.AddWithValue("p_page", page);
            cmd.Parameters.AddWithValue("p_order_column", orderColumn);
            cmd.Parameters.AddWithValue("p_order_direction", orderDirection);
            cmd.Parameters.AddWithValue("p_is_active", (object?)isActive ?? DBNull.Value);

            // OUTPUT: TOTAL COUNT
            var totalParam = new NpgsqlParameter("c_total_numbers", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = ParameterDirection.InputOutput,
                Value = 0
            };
            cmd.Parameters.Add(totalParam);

            // OUTPUT: CURSOR
            var cursorParam = new NpgsqlParameter("refcursor", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "job_post_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            // EXECUTE PROCEDURE
            await cmd.ExecuteNonQueryAsync();

            // FETCH CURSOR DATA
            var posts = (await conn.QueryAsync<JobPost>(
                "FETCH ALL IN \"job_post_cursor\"",
                transaction: tran
            )).ToList();

            tran.Commit();

            return new JobPostListResponse
            {
                TotalNumbers = (int)totalParam.Value,
                JobPosts = posts
            };
        }
        #endregion

        #region Get Job Post By Id
        public async Task<JobPostByIdResponse> GetJobPostByIdAsync(Guid id)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetJobPostById}(@p_id, @ref)",
                (NpgsqlConnection)conn
            );

            cmd.Transaction = (NpgsqlTransaction)tran;

            // Input
            cmd.Parameters.AddWithValue("p_id", id);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "job_post_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            await cmd.ExecuteNonQueryAsync();

            var posts = conn.Query<JobPost>(
                "FETCH ALL IN \"job_post_cursor\"",
                transaction: tran
            ).ToList();

            var post = posts.FirstOrDefault();

            tran.Commit();

            return new JobPostByIdResponse
            {
                JobPost = post
            };
        }
        #endregion

        #region Publish Job Post (Reuse Job Template)
        public async Task<JobPostCreateResponse> PublishJobPostAsync(JobPostCreateRequest request)
        {
            _logger.LogInformation(
                "Executing PublishJobPostAsync for JobTemplateId={TemplateId}",
                request.JobTemplateId
            );

            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst("user_id")?.Value;
            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // INPUT
            parameters.Add("p_job_template_id", request.JobTemplateId);
            parameters.Add("p_created_by", createdBy);
            parameters.Add("p_valid_months", request.ValidMonths);

            // OUTPUT
            parameters.Add("o_job_post_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            await conn.ExecuteAsync(
                StoreProcedure.JobPostPublish,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var jobPostId = parameters.Get<Guid?>("o_job_post_id") ?? Guid.Empty;

            return new JobPostCreateResponse
            {
                JobPostId = jobPostId
            };
        }
        #endregion

        #region Delete Job Post
        public async Task DeleteJobPostAsync(JobPostDeleteRequest request)
        {
            _logger.LogInformation("Executing DeleteJobPostAsync for Id={Id}", request.Id);

            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst("user_id")?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            parameters.Add("p_id", request.Id);
            parameters.Add("p_updated_by", updatedBy);

            await conn.ExecuteAsync(
                StoreProcedure.JobPostDelete,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        #endregion

        #endregion
    }
}
