using Dapper;

using InterviewService.Api.Helpers;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.ViewModels.Request.Interviewer_setting;
using InterviewService.Api.ViewModels.Response.Interviewer_setting;

using Npgsql;

using System.Data;
using System.Security.Claims;

namespace InterviewService.Api.Infrastructure.Repositories
{
    public class InterviewerSettingRepository(ILogger<InterviewerSettingRepository> _logger, IHttpContextAccessor _contextAccessor, IDbConnectionFactory _dbFactory) : IInterviewerSettingRepository
    {
        public async Task<InterviewerSettingCreateUpdateResponseViewModel> CreateUpdateInterviewerSettingAsync(InterviewerSettingCreateUpdateRequestViewModel request)
        {
            _logger.LogInformation("Executing InterviewerSettingCreateUpdate stored procedure for Name={Name}", request.Name);

            using var conn = _dbFactory.CreateConnection();
            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_id", request.Id);
            parameters.Add("p_name", request.Name);
            parameters.Add("p_value", request.Value);
            parameters.Add("p_interviewer_id", request.Interviewer_id);
            parameters.Add("p_created_by", createdBy);

            // Output
            parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.InterviewerSettingCreateUpdate,
                parameters,
                commandType: CommandType.StoredProcedure
            );
            // Output values
            var outOrgId = parameters.Get<Guid?>("o_id") ?? Guid.Empty;

            // Map output parameters to entity
            var data = new InterviewerSettingCreateUpdateResponseViewModel
            {
                Id = outOrgId,
                Name = request.Name,
                Value = request.Value,
                Interviewer_id = request.Interviewer_id
            };
            // Return in ResponseViewModel wrapper
            return data;
        }

        public async Task DeleteInterviewerSettingAsync(InterviewerSettingDeleteRequestViewModel request)
        {
            _logger.LogInformation("Executing DeleteInterviewerSettingAsync stored procedure for Id={Id}", request.Id);

            var interviewIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(interviewIdClaim) ? (Guid?)null : Guid.Parse(interviewIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_id", request.Id);
            parameters.Add("p_updated_by", updatedBy);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.InterviewerSettingDelete,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<InterviewerSettingListResponseViewModel> GetInterviewerSettingsAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetInterviewerSettingList}(@p_search, @p_length, @p_page, @p_order_column, @p_order_direction, @p_is_active, @o_total_numbers, @ref)",
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

            var interviewersSetting = conn.Query<InterviewerSettingData>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            // Map output parameters to entity
            var data = new InterviewerSettingListResponseViewModel
            {
                TotalNumbers = (int)totalParam.Value,
                InterviewerSettingData = interviewersSetting
            };

            // Return in ResponseViewModel wrapper
            return data;
        }

        public async Task<InterviewerSettingData> GetInterviewerSettingByIdAsync(Guid? Id)
        {
            if (Id == null)
                return null;

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();
            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetInterviewerSettingById}(@p_id, @ref)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            cmd.Parameters.AddWithValue("p_id", Id.Value);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            await cmd.ExecuteNonQueryAsync();

            var result = conn.QueryFirstOrDefault<InterviewerSettingData>(
                "FETCH ALL IN \"my_cursor\"",
                transaction: tran
            );

            tran.Commit();

            return result;
        }
    }
}
