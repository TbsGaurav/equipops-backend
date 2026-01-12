using Dapper;

using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.ViewModels.Request.Interview_Form;
using InterviewService.Api.ViewModels.Response.Interview_Form;

using Npgsql;

using System.Data;
using System.Security.Claims;
using System.Text.Json;

namespace InterviewService.Api.Infrastructure.Repositories
{
    public class InterviewFormRepository : IInterviewFormRepository
    {
        private readonly ILogger<InterviewFormRepository> _logger;
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IHttpContextAccessor _contextAccessor;

        public InterviewFormRepository(IDbConnectionFactory dbFactory, ILogger<InterviewFormRepository> logger, IHttpContextAccessor contextAccessor)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        #region Interview Form
        public async Task<InterviewFormCreateUpdateResponseViewModel> InterviewFormCreateAsync(InterviewFormRequestViewModel request)
        {
            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updated_by = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("_id", request.id, DbType.Guid);
            parameters.Add("_interview_id", request.interview_id);
            parameters.Add("_form_json_data", request.form_json_data.GetRawText(), DbType.String);
            parameters.Add("_updated_by", updated_by, DbType.Guid);

            // Output
            parameters.Add("_return_id", dbType: DbType.Guid, direction: ParameterDirection.InputOutput);

            await conn.ExecuteAsync(
                "CALL interviews.sp_interview_form_create_update(@_return_id::uuid, @_id::uuid, @_interview_id::uuid, @_form_json_data::json, @_updated_by::uuid)",
                parameters
            );

            var data = new InterviewFormCreateUpdateResponseViewModel
            {
                id = parameters.Get<Guid?>("_return_id"),
                interview_id = request.interview_id,
                form_json_data = request.form_json_data
            };

            // Return in ResponseViewModel wrapper
            return data;
        }


        public async Task<InterviewFormListResponseViewModel> InterviewFormListAsync(string? search, bool? Is_Active, int length, int page, string orderColumn, string orderDirection)
        {
            await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var cursorName = "mycursor";

            //string value = null;
            //int len = value.Length; // will throw

            await using var cmd = new NpgsqlCommand("CALL interviews.sp_interview_form_get_list(@p_search, @p_is_active, @p_length, @p_page, @p_order_column, @p_order_direction, @o_total_records, @ref)", conn, tran);

            cmd.Parameters.AddWithValue("p_search", NpgsqlTypes.NpgsqlDbType.Text, search ?? "");
            cmd.Parameters.AddWithValue("p_is_active", NpgsqlTypes.NpgsqlDbType.Boolean, Is_Active ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_length", NpgsqlTypes.NpgsqlDbType.Integer, length);
            cmd.Parameters.AddWithValue("p_page", NpgsqlTypes.NpgsqlDbType.Integer, page);
            cmd.Parameters.AddWithValue("p_order_column", NpgsqlTypes.NpgsqlDbType.Text, orderColumn);
            cmd.Parameters.AddWithValue("p_order_direction", NpgsqlTypes.NpgsqlDbType.Text, orderDirection);

            // OUT parameters
            var totalParam = new NpgsqlParameter("o_total_records", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = ParameterDirection.InputOutput,
                Value = 0
            };
            cmd.Parameters.Add(totalParam);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = cursorName
            };
            cmd.Parameters.Add(cursorParam);

            // Execute the procedure
            await cmd.ExecuteNonQueryAsync();

            // Fetch cursor data inside the same transaction
            InterviewFormListResponseViewModel list = new InterviewFormListResponseViewModel();
            list.TotalNumbers = (int)cmd.Parameters["o_total_records"].Value;
            await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran))
            await using (var reader = await fetchCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.interviewForm.Add(new InterviewFormResponseViewModel
                    {
                        id = reader.GetGuid(0),
                        interview_id = reader.GetGuid(1),
                        //form_json_data = reader.GetString(2),
                        form_json_data = reader.IsDBNull(2) ? default : JsonSerializer.Deserialize<JsonElement>(reader.GetString(2)),
                        created_by = reader.IsDBNull(3) ? null : reader.GetGuid(3),
                        created_date = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                        updated_by = reader.IsDBNull(5) ? null : reader.GetGuid(5),
                        updated_date = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                        is_delete = reader.GetBoolean(7),
                        is_active = reader.GetBoolean(8)
                    });
                }
            }

            await tran.CommitAsync();
            return list;
        }

        public async Task<InterviewFormDeleteResponseViewModel> InterviewFormDeleteAsync(InterviewFormDeleteRequestViewModel request)
        {
            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updated_by = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("_id", request.id);
            parameters.Add("_updated_by", updated_by);
            parameters.Add("_is_delete", true);
            parameters.Add("_is_active", false);

            // Output
            parameters.Add("_return_id", dbType: DbType.Guid, direction: ParameterDirection.InputOutput);
            parameters.Add("_return_updateddate", dbType: DbType.DateTime, direction: ParameterDirection.InputOutput);

            // Execute procedure
            await conn.ExecuteAsync(
                "CALL interviews.sp_interview_form_delete(@_return_id, @_id, @_updated_by)",
                parameters
            );

            // Map output parameters to entity
            var data = new InterviewFormDeleteResponseViewModel
            {
                id = parameters.Get<Guid?>("_return_id")
            };

            // Return in ResponseViewModel wrapper
            return data;
        }

        public async Task<InterviewFormResponseViewModel> InterviewFormByIdAsync(Guid? id)
        {
            await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var cursorName = "mycursor";

            await using var cmd = new NpgsqlCommand("CALL interviews.sp_interview_form_get_byid(@_id, @ref)", conn, tran);

            cmd.Parameters.AddWithValue("_id", NpgsqlTypes.NpgsqlDbType.Uuid, id);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = cursorName
            };
            cmd.Parameters.Add(cursorParam);

            // Execute the procedure
            await cmd.ExecuteNonQueryAsync();

            // Fetch cursor data inside the same transaction
            InterviewFormResponseViewModel interviewFormResponseViewModel = new InterviewFormResponseViewModel();
            await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran))
            await using (var reader = await fetchCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    interviewFormResponseViewModel.id = reader.GetGuid(0);
                    interviewFormResponseViewModel.interview_id = reader.GetGuid(1);
                    //interviewFormResponseViewModel.form_json_data = reader.GetString(2);
                    interviewFormResponseViewModel.form_json_data = reader.IsDBNull(2) ? default : JsonSerializer.Deserialize<JsonElement>(reader.GetString(2));
                    interviewFormResponseViewModel.created_by = reader.IsDBNull(3) ? null : reader.GetGuid(3);
                    interviewFormResponseViewModel.created_date = reader.IsDBNull(4) ? null : reader.GetDateTime(4);
                    interviewFormResponseViewModel.updated_by = reader.IsDBNull(5) ? null : reader.GetGuid(5);
                    interviewFormResponseViewModel.updated_date = reader.IsDBNull(6) ? null : reader.GetDateTime(6);
                    interviewFormResponseViewModel.is_delete = reader.GetBoolean(7);
                    interviewFormResponseViewModel.is_active = reader.GetBoolean(8);
                }
            }

            await tran.CommitAsync();
            return interviewFormResponseViewModel;
        }
        #endregion
    }
}
