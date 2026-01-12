using Dapper;

using InterviewService.Api.Helpers;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.ViewModels.Request.Interview_Type;
using InterviewService.Api.ViewModels.Response.Interview;

using Npgsql;

using System.Data;
using System.Security.Claims;

namespace InterviewService.Api.Infrastructure.Repositories
{
    public class InterviewTypeRepository : IInterviewTypeRepository
    {
        private readonly ILogger<InterviewTypeRepository> _logger;
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IHttpContextAccessor _contextAccessor;

        public InterviewTypeRepository(IDbConnectionFactory dbFactory, ILogger<InterviewTypeRepository> logger, IHttpContextAccessor contextAccessor)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        #region Get Interview Types List
        public async Task<InterviewTypeListResponseViewModel> GetInterviewTypesAsync(
            string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetInterviewTypeList}(@p_search, @p_length, @p_page, @p_order_column, @p_order_direction, @p_is_active, @o_total_numbers, @ref)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            var parameters = new DynamicParameters();

            // Input
            cmd.Parameters.AddWithValue("p_search", (object?)Search ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_length", Length);
            cmd.Parameters.AddWithValue("p_page", Page);
            cmd.Parameters.AddWithValue("p_order_column", OrderColumn);
            cmd.Parameters.AddWithValue("p_order_direction", OrderDirection);
            cmd.Parameters.AddWithValue("p_is_active", (object?)IsActive ?? DBNull.Value);

            var totalParam = new NpgsqlParameter("o_total_numbers", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = ParameterDirection.InputOutput,
                Value = 0
            };
            cmd.Parameters.Add(totalParam);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            // Execute procedure
            cmd.ExecuteNonQuery();

            var interviewTypes = conn.Query<InterviewTypeData>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            // Map output parameters to entity
            var data = new InterviewTypeListResponseViewModel
            {
                TotalNumbers = (int)totalParam.Value,
                InterviewTypeData = interviewTypes
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

        #region Get Interview Type By Id
        public async Task<InterviewTypeByIdResponseViewModel> GetInterviewTypeByIdAsync(Guid Id)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetInterviewTypeById}(@p_id, @ref)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            var parameters = new DynamicParameters();

            // Input
            cmd.Parameters.AddWithValue("p_id", Id);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            // Execute procedure
            cmd.ExecuteNonQuery();

            var interviewTypes = conn.Query<InterviewType>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            var interviewType = interviewTypes.FirstOrDefault();

            tran.Commit();

            // Map output parameters to entity

            var data = new InterviewTypeByIdResponseViewModel
            {
                InterviewType = interviewType
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

        #region Create/Update Interview Type
        public async Task<InterviewTypeCreateUpdateResponseViewModel> CreateUpdateInterviewTypeAsync(InterviewTypeCreateUpdateRequestViewModel request)
        {
            _logger.LogInformation("Executing InterviewTypeCreateUpdate stored procedure for InterviewType Interview_Type={Interview_Type}", request.Interview_Type);
            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst("user_id")?.Value;
            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_id", request.Id);
            parameters.Add("p_interview_type", request.Interview_Type);
            parameters.Add("p_created_by", createdBy);

            // Output
            parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.InterviewTypeCreateUpdate,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            // Output values
            var outOrgId = parameters.Get<Guid?>("o_id") ?? Guid.Empty;

            // Map output parameters to entity
            var data = new InterviewTypeCreateUpdateResponseViewModel
            {
                Id = outOrgId,
                Interview_Type = request.Interview_Type,
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

        #region Delete Interview Type
        public async Task DeleteInterviewTypeAsync(InterviewTypeDeleteRequestViewModel request)
        {
            _logger.LogInformation("Executing DeleteInterviewTypeAsync stored procedure for Id={Id}", request.Id);
            var interviewTypeIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(interviewTypeIdClaim) ? (Guid?)null : Guid.Parse(interviewTypeIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_id", request.Id);
            parameters.Add("p_updated_by", updatedBy);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.InterviewTypeDelete,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        #endregion
    }
}