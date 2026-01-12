using Dapper;

using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.ViewModels.Request.Interview_Que;
using InterviewService.Api.ViewModels.Response.Interview_Que;

using Npgsql;

using System.Data;
using System.Security.Claims;

namespace InterviewService.Api.Infrastructure.Repositories
{
    public class InterviewQueRepository : IInterviewQueRepository
    {
        private readonly ILogger<InterviewQueRepository> _logger;
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IHttpContextAccessor _contextAccessor;

        public InterviewQueRepository(IDbConnectionFactory dbFactory, ILogger<InterviewQueRepository> logger, IHttpContextAccessor contextAccessor)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        #region Interview Que
        public async Task<InterviewQueCreateUpdateResponseViewModel> InterviewQueCreateAsync(InterviewQueRequestViewModel request)
        {
            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updated_by = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            //// Input
            //parameters.Add("_id", request.id);
            //parameters.Add("_interview_id", request.interview_id);
            //parameters.Add("_question", request.question);
            //parameters.Add("_depth_level", request.depth_level);
            //parameters.Add("_description", request.description);
            //parameters.Add("_created_by", updated_by);
            //parameters.Add("_updated_by", updated_by);
            //parameters.Add("_is_delete", false);
            //parameters.Add("_is_active", true);

            //// Output
            //parameters.Add("_return_id", dbType: DbType.Guid, direction: ParameterDirection.InputOutput);
            //parameters.Add("_return_createddate", dbType: DbType.DateTime, direction: ParameterDirection.InputOutput);
            //parameters.Add("_return_updateddate", dbType: DbType.DateTime, direction: ParameterDirection.InputOutput);

            //// Execute procedure
            //await conn.ExecuteAsync(
            //    "CALL interviews.sp_interview_que_create_update(@_return_id, @_return_createddate,@_return_updateddate, @_id, @_interview_id, @_question, @_depth_level, @_description, @_updated_by)",
            //    parameters
            //);

            parameters.Add("p_id", null); // as per procedure signature
            parameters.Add("p_interview_id", request.interview_id);
            parameters.Add("p_updated_by", updated_by);

            //parameters.Add("p_bulk_data", request.questions.ToArray(), dbType: (DbType?)NpgsqlTypes.NpgsqlDbType.Array);
            parameters.Add("p_bulk_data", request.questions.ToArray());
            await conn.ExecuteAsync(
                "interviews.sp_interview_question_bulk_create_update",
                parameters,
                commandType: CommandType.StoredProcedure
            );



            // Map output parameters to entity
            var data = new InterviewQueCreateUpdateResponseViewModel
            {
                //interview_id = request.interview_id,
                //questions = request.questions
            };

            // Return in ResponseViewModel wrapper
            return data;
        }


        public async Task<InterviewQueListResponseViewModel> InterviewQueListAsync(string? search, bool? Is_Active, int length, int page, string orderColumn, string orderDirection)
        {
            await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var cursorName = "mycursor";

            //string value = null;
            //int len = value.Length; // will throw

            await using var cmd = new NpgsqlCommand("CALL interviews.sp_interview_que_get_list(@p_search, @p_is_active, @p_length, @p_page, @p_order_column, @p_order_direction, @o_total_records, @ref)", conn, tran);

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
            InterviewQueListResponseViewModel list = new InterviewQueListResponseViewModel();
            list.TotalNumbers = (int)cmd.Parameters["o_total_records"].Value;
            await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran))
            await using (var reader = await fetchCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.interviewQue.Add(new InterviewQueResponseViewModel
                    {
                        id = reader.GetGuid(0),
                        interview_id = reader.GetGuid(1),
                        question = reader.GetString(2),
                        depth_level = reader.GetInt32(3),
                        //description = reader.GetString(4),
                        created_by = reader.IsDBNull(5) ? null : reader.GetGuid(5),
                        created_date = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                        updated_by = reader.IsDBNull(7) ? null : reader.GetGuid(7),
                        updated_date = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                        is_delete = reader.GetBoolean(9),
                        is_active = reader.GetBoolean(10)
                    });
                }
            }

            await tran.CommitAsync();
            return list;
        }

        public async Task<InterviewQueDeleteResponseViewModel> InterviewQueDeleteAsync(InterviewQueDeleteRequestViewModel request)
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
                "CALL interviews.sp_interview_que_delete(@_return_id, @_return_updateddate, @_id, @_updated_by, @_is_delete, @_is_active)",
                parameters
            );

            // Map output parameters to entity
            var data = new InterviewQueDeleteResponseViewModel
            {
                id = parameters.Get<Guid?>("_return_id")
            };

            // Return in ResponseViewModel wrapper
            return data;
        }

        public async Task<InterviewQueResponseViewModel> InterviewQueByIdAsync(Guid? id)
        {
            await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var cursorName = "mycursor";

            await using var cmd = new NpgsqlCommand("CALL interviews.sp_interview_que_get_byid(@_id, @ref)", conn, tran);

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
            InterviewQueResponseViewModel interviewQueResponseViewModel = new InterviewQueResponseViewModel();
            await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran))
            await using (var reader = await fetchCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    interviewQueResponseViewModel.id = reader.GetGuid(0);
                    interviewQueResponseViewModel.interview_id = reader.GetGuid(1);
                    interviewQueResponseViewModel.question = reader.GetString(2);
                    interviewQueResponseViewModel.depth_level = reader.GetInt32(3);
                    //interviewQueResponseViewModel.description = reader.GetString(4);
                    interviewQueResponseViewModel.created_by = reader.IsDBNull(5) ? null : reader.GetGuid(5);
                    interviewQueResponseViewModel.created_date = reader.IsDBNull(6) ? null : reader.GetDateTime(6);
                    interviewQueResponseViewModel.updated_by = reader.IsDBNull(7) ? null : reader.GetGuid(7);
                    interviewQueResponseViewModel.updated_date = reader.IsDBNull(8) ? null : reader.GetDateTime(8);
                    interviewQueResponseViewModel.is_delete = reader.GetBoolean(9);
                    interviewQueResponseViewModel.is_active = reader.GetBoolean(10);
                }
            }

            await tran.CommitAsync();
            return interviewQueResponseViewModel;
        }

        public async Task<List<InterviewQuestionBulkDto>> GetInterviewQuestionsByInterviewIdAsync(Guid interviewId)
        {
            using var conn = _dbFactory.CreateConnection();

            var sql = @"
                    SELECT
                        id,
                        question AS Question,
                        depth_level
                    FROM interviews.interviews_question
                    WHERE interview_id = @interviewId
                      AND is_delete = false
                    ORDER BY created_date;
                ";

            var result = await conn.QueryAsync<InterviewQuestionBulkDto>(
                sql,
                new { interviewId }
            );

            return result.ToList();
        }


        #endregion
    }
}
