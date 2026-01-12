using Common.Services.ViewModels.RetellAI;

using Dapper;

using InterviewService.Api.Helpers;
using InterviewService.Api.Helpers.EncryptionHelpers.Handlers;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.ViewModels.Request.Interviewer;
using InterviewService.Api.ViewModels.Response.Interviewer;

using Newtonsoft.Json;

using Npgsql;

using System.Data;

namespace InterviewService.Api.Infrastructure.Repositories
{
    public class InterviewerRepository(ILogger<InterviewerRepository> _logger, IDbConnectionFactory _dbFactory, IHttpContextAccessor _contextAccessor, EncryptionHelper encryptionHelper, RetellAIEndpoints retellUrl) : IInterviewerRepository
    {
        #region Interviewer Create update 
        public async Task<InterviewerCreateUpdateResponseViewModel> CreateInterviewerAsync(InterviewerDataCreateRequestViewModel request)
        {
            _logger.LogInformation("Executing InterviewerCreateUpdate stored procedure for Record_url={Record_url}", request.Record_url);

            using var conn = _dbFactory.CreateConnection();
            var userIdClaim = _contextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value;
            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_name", request.Name);
            parameters.Add("p_agent_id", request.Agent_id);
            parameters.Add("p_voice_id", request.Voice_id);
            parameters.Add("p_record_url", request.Record_url);
            parameters.Add("p_avatar_url", request.Avatar_url);
            parameters.Add("p_organization_id", request.Organization_id);
            parameters.Add("p_created_by", createdBy);

            // Output
            parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.InterviewerCreate,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var outOrgId = parameters.Get<Guid?>("o_id") ?? Guid.Empty;

            var data = new InterviewerCreateUpdateResponseViewModel
            {
                Id = outOrgId,
                Name = request.Name,
                Agent_id = request.Agent_id,
                Voice_id = request.Voice_id,
                Avatar_url = request.Avatar_url,
                Record_url = request.Record_url,
                Organization_id = request.Organization_id
            };

            return data;
        }
        #endregion

        #region Interviewer Delete
        public async Task DeleteInterviewerAsync(InterviewerDeleteRequestViewModel request)
        {
            _logger.LogInformation("Executing DeleteInterviewerAsync stored procedure for Id={Id}", request.Id);
            var userIdClaim = _contextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_id", request.Id);
            parameters.Add("p_updated_by", updatedBy);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.InterviewerDelete,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        #endregion

        #region Get Interviewer By Id
        public async Task<InterviewerListResponseViewModel> GetInterviewerByIdAsync(Guid? Id)
        {
            if (Id == null)
                return null;

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();
            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetInterviewerById}(@p_id, @ref)",
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

            var result = conn.QueryFirstOrDefault<InterviewerListResponseViewModel>(
                "FETCH ALL IN \"my_cursor\"",
                transaction: tran
            );

            tran.Commit();

            return result;
        }
        #endregion

        #region Get Interviewer List
        public async Task<List<InterviewerData>> GetInterviewersAsync(Guid? organizationId)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();
            string cursorName = "interviewer_cursor";

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetInterviewerList}(@p_organization_id, @refcursor)",
                (NpgsqlConnection)conn
            );

            cmd.Parameters.Add(new NpgsqlParameter("p_organization_id", NpgsqlTypes.NpgsqlDbType.Uuid)
            {
                Value = organizationId ?? (object)DBNull.Value
            });

            cmd.Parameters.Add(new NpgsqlParameter("refcursor", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = cursorName
            });

            await cmd.ExecuteNonQueryAsync();

            var interviewers = conn.Query<InterviewerData>(
                $"FETCH ALL IN \"{cursorName}\"",
                transaction: tran
            ).ToList();

            tran.Commit();

            return interviewers;
        }
        #endregion

        #region Get retell llm key
        public async Task<string?> Get_retell_LLM_key(Guid organizationId)
        {
            using var conn = _dbFactory.CreateConnection();

            var sql = @"
              SELECT value
              FROM master.organization_Setting
              WHERE organization_id = @OrgId 
                AND key = 'retell_llm_key'
              LIMIT 1;
            ";

            var encrypted = await conn.QueryFirstOrDefaultAsync<string>(sql, new { OrgId = organizationId });

            if (string.IsNullOrEmpty(encrypted))
                return null;

            var decrypted = encryptionHelper.DecryptFromReact(encrypted);

            return decrypted;
        }
        #endregion

        #region Get retell ai key
        public async Task<string?> GetRetellAiKey(Guid organizationId)
        {
            using var conn = _dbFactory.CreateConnection();

            var sql = @"
              SELECT value
              FROM master.organization_Setting
              WHERE organization_id = @OrgId 
                AND key = 'retell_ai_api_key'
              LIMIT 1;
            ";

            var encrypted = await conn.QueryFirstOrDefaultAsync<string>(sql, new { OrgId = organizationId });

            if (string.IsNullOrEmpty(encrypted))
                return null;

            var decrypted = encryptionHelper.DecryptFromReact(encrypted);

            return decrypted;
        }
        #endregion

        #region Get retell voice By Id
        public async Task<RetellAiVoiceModel?> GetVoiceById(string VoiceId, string RetellApiKey)
        {
            using var conn = _dbFactory.CreateConnection();

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", RetellApiKey);

            //call retell api to get voice by id
            var url = retellUrl.GetVoiceById(VoiceId);
            var response = await http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to fetch voice. Status: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();

            var model = JsonConvert.DeserializeObject<RetellAiVoiceModel>(json);

            return model;
        }

        public async Task<InterviewerCreateUpdateResponseViewModel> UpdateInterviewerAsync(InterviewerDataUpdateRequestViewModel request)
        {
            _logger.LogInformation("Executing InterviewerCreateUpdate stored procedure for Record_url={Record_url}", request.Record_url);

            using var conn = _dbFactory.CreateConnection();
            var userIdClaim = _contextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_id", request.Id);
            parameters.Add("p_name", request.Name);
            parameters.Add("p_agent_id", request.Agent_id);
            parameters.Add("p_voice_id", request.Voice_id);
            parameters.Add("p_record_url", request.Record_url);
            parameters.Add("p_avatar_url", request.Avatar_url);
            parameters.Add("p_updated_by", updatedBy);

            // Output
            parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.InterviewerUpdate,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var outOrgId = parameters.Get<Guid?>("o_id") ?? Guid.Empty;

            var data = new InterviewerCreateUpdateResponseViewModel
            {
                Id = outOrgId,
                Name = request.Name,
                Agent_id = request.Agent_id,
                Voice_id = request.Voice_id,
                Avatar_url = request.Avatar_url,
                Record_url = request.Record_url,
            };

            return data;
        }
        #endregion

        #region Get open ai key
        public async Task<string?> GetOpenAiKey(Guid organizationId)
        {
            using var conn = _dbFactory.CreateConnection();

            var sql = @"
              SELECT value
              FROM master.organization_Setting
              WHERE organization_id = @OrgId 
                AND key = 'open_ai_api_key'
              LIMIT 1;
            ";

            var encrypted = await conn.QueryFirstOrDefaultAsync<string>(sql, new { OrgId = organizationId });

            if (string.IsNullOrEmpty(encrypted))
                return null;

            var decrypted = encryptionHelper.DecryptFromReact(encrypted);

            return decrypted;
        }
        #endregion
    }
}
