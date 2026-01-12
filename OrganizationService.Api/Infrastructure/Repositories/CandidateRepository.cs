using Dapper;

using Newtonsoft.Json.Linq;

using Npgsql;

using OrganizationService.Api.Helpers;
using OrganizationService.Api.Helpers.EncryptionHelpers.Handlers;
using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.ViewModels.Request.Candidate;
using OrganizationService.Api.ViewModels.Request.User;
using OrganizationService.Api.ViewModels.Response.Candidate;

using System.Data;
using System.Security.Claims;
using System.Text.Json;

namespace OrganizationService.Api.Infrastructure.Repositories
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly ILogger<CandidateRepository> _logger;
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly EncryptionHelper _encryptionHelper;
        private readonly string _gatewayBaseUrl;
        private readonly string _gatewayInterviewLinkBaseUrl;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;
        public CandidateRepository(IDbConnectionFactory dbFactory, ILogger<CandidateRepository> logger, IHttpContextAccessor contextAccessor, IConfiguration configuration, EncryptionHelper encryptionHelper, HttpClient client)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _contextAccessor = contextAccessor;
            _configuration = configuration;
            _encryptionHelper = encryptionHelper;
            _gatewayBaseUrl = _configuration["ImageUploadSettings:BaseUrl"] ?? string.Empty;
            _gatewayInterviewLinkBaseUrl = _configuration["InterviewLinkSend:BaseUrl"] ?? string.Empty;
            _client = client;
        }

        #region Get Candidates List
        public async Task<CandidateListResponseViewModel> GetCandidatesAsync(
            string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            var organizationIdClaim = _contextAccessor.HttpContext?.User?.FindFirst("organization_id")?.Value;

            var organizationId = string.IsNullOrWhiteSpace(organizationIdClaim) ? (Guid?)null : Guid.Parse(organizationIdClaim);

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetCandidateList}(@p_search, @p_length, @p_page, @p_order_column, @p_order_direction, @p_is_active, @p_organization_id, @c_total_numbers, @ref)",
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
            cmd.Parameters.AddWithValue("p_organization_id", organizationId);

            var totalParam = new NpgsqlParameter("c_total_numbers", NpgsqlTypes.NpgsqlDbType.Integer)
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

            var candidates = conn.Query<CandidateData>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            // Map output parameters to entity
            var data = new CandidateListResponseViewModel
            {
                TotalNumbers = (int)totalParam.Value,
                CandidateData = candidates
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

        #region Get Candidate By Id
        public async Task<CandidateByIdResponseViewModel> GetCandidateByIdAsync(Guid Id)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetCandidateById}(@p_id, @ref)",
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

            var candidates = conn.Query<Candidate>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            var candidate = candidates.FirstOrDefault();

            tran.Commit();

            // Map output parameters to entity

            var data = new CandidateByIdResponseViewModel
            {
                Candidate = candidate
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

        #region Create/Update Candidate
        public async Task<CandidateCreateUpdateResponseViewModel> CreateUpdateCandidateAsync(CandidateCreateUpdateRequestViewModel request, string Resume_url, string TotalExperience, string Skill, JsonElement json, decimal matchScore)
        {

            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
            
            Guid? organization_id = await GetOrganizationIdByInterviewId(request.json_form_data.InterviewId);
            
            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_id", request.json_form_data.Id, DbType.Guid);
            parameters.Add("p_interview_id", request.json_form_data.InterviewId);
            parameters.Add("p_name", request.json_form_data.Name);
            parameters.Add("p_experience", TotalExperience);
            parameters.Add("p_skill", Skill);
            parameters.Add("p_description", "");
            parameters.Add("p_resume_url", Resume_url);
            parameters.Add("p_created_by", createdBy, DbType.Guid);
            parameters.Add("p_email", request.json_form_data.Email);
            parameters.Add("p_phone", request.json_form_data.Phone_Number);
            parameters.Add("p_json", json.GetRawText(), DbType.String);
            parameters.Add("p_matchScore", matchScore, DbType.Decimal);
            parameters.Add("p_upload_type", request.json_form_data.CandidateUploadType, DbType.Int32);
            parameters.Add("p_organization_id", organization_id, DbType.Guid);

            //Output
            parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            // Execute procedure
            //await conn.ExecuteAsync(
            //    StoreProcedure.CandidateCreateUpdate,
            //    parameters,
            //    commandType: CommandType.StoredProcedure
            //);

            await conn.ExecuteAsync(
               "CALL master.sp_candidate_detail_create_update(" +
               "@p_id::uuid, " +
               "@p_interview_id::uuid, " +
               "@p_name::character varying, " +
               "@p_experience::character varying, " +
               "@p_skill::text, " +
               "@p_description::text, " +
               "@p_resume_url::text, " +
               "@p_created_by::uuid, " +
               "@p_email::character varying, " +
               "@p_phone::character varying, " +
               "@p_json::json, " +
               "@p_matchScore::numeric(10,2), " +
               "@p_upload_type::integer, " +
               "@p_organization_id::uuid, " +
               "null)",
               parameters
           );

            // Output values
            var outOrgId = parameters.Get<Guid?>("o_id") ?? Guid.Empty;

            // Map output parameters to entity
            var data = new CandidateCreateUpdateResponseViewModel
            {
                Id = outOrgId,
                InterviewId = request.json_form_data.InterviewId,
                Name = request.json_form_data.Name,
                Experience = TotalExperience,
                Skill = Skill,
                ResumeUrl = Resume_url,
                Email = request.json_form_data.Email,
                Phone_Number = request.json_form_data.Phone_Number
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

        #region Delete Candidate
        public async Task DeleteCandidateAsync(CandidateDeleteRequestViewModel request)
        {
            _logger.LogInformation("Executing DeleteCandidateAsync stored procedure for Id={Id}", request.Id);

            var candidateIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(candidateIdClaim) ? (Guid?)null : Guid.Parse(candidateIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_id", request.Id);
            parameters.Add("p_updated_by", updatedBy);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.CandidateDelete,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        #endregion

        #region Get open ai key
        public async Task<string?> GetOpenAiKey(Guid organizationId, Guid interviewId)
        {
            using var conn = _dbFactory.CreateConnection();

            var sql = "";
            var encrypted = "";
            if (organizationId == Guid.Empty)
            {
                sql = @"
                  SELECT os.value
                  FROM master.organization_Setting as os
                   inner join interviews.interviews as i on i.organization_id = os.organization_id
                  WHERE i.id = @OrgId 
                    AND os.key = 'open_ai_api_key'
                  LIMIT 1;
                ";
                encrypted = await conn.QueryFirstOrDefaultAsync<string>(sql, new { OrgId = interviewId });
            }
            else
            {
                sql = @"
                  SELECT value
                  FROM master.organization_Setting
                  WHERE organization_id = @OrgId 
                    AND key = 'open_ai_api_key'
                  LIMIT 1;
                ";
                encrypted = await conn.QueryFirstOrDefaultAsync<string>(sql, new { OrgId = organizationId });
            }

            if (string.IsNullOrEmpty(encrypted))
                return null;

            var decrypted = _encryptionHelper.DecryptFromReact(encrypted);

            return decrypted;
        }
        #endregion

        #region Get skills
        public async Task<List<string>?> GetSkillsAsync(Guid interviewId)
        {
            using var conn = _dbFactory.CreateConnection();

            var sql = @"
                        SELECT form_json_data
                        FROM interviews.interviews_form
                        WHERE interview_id = @Id 
                          AND is_delete = false 
                          AND is_active = true
                        LIMIT 1;
                    ";

            var formJsonData = await conn.QueryFirstOrDefaultAsync<string>(
                sql,
                new { Id = interviewId }
            );

            if (string.IsNullOrWhiteSpace(formJsonData))
                return null;

            using var document = JsonDocument.Parse(formJsonData);

            // 🔹 Navigate safely: skills -> options
            if (!document.RootElement.TryGetProperty("skills", out var skillsElement))
                return null;

            if (!skillsElement.TryGetProperty("options", out var optionsElement))
                return null;

            var skills = optionsElement
                .EnumerateArray()
                .Select(x => x.GetString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return skills;
        }

        #endregion

        #region Generate Interview Token
        public async Task<string> CreateInterviewTokenAsync(InterviewTokenRequestViewModel request, bool IsDirect)
        {
            try
            {
                var interviewer = await GetCandidateAsync(request.Interview_id);
            var obj = JObject.Parse(interviewer); 
            var organizationId = obj["data"]?["interview"]?["organization_Id"]?.ToObject<Guid>();

            Guid interviewInvitationId = await CreateInterviewInvitationAsync(organizationId, request.Interview_id, request.Candidate_id, request.Interview_date, IsDirect);

            //var interviewerId = interviewer.Interview?.Interviewer_Id;
            //var organizationId = interviewer.Interview?.Organization_Id;
            var payload = new
            {
                CandidateInterviewInvitationId = interviewInvitationId,
                InterviewDate= request.Interview_date
            };
            var payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var encryptedToken = _encryptionHelper.EncryptForReact(payloadJson);
            await AddInterviewToken(new UserTokenRequestViewModel
            {
                Token = encryptedToken,
                Token_data = payloadJson,
                Token_type = "InterviewInvitation",
                Status = "1",
                Token_expiry = request.Interview_date.AddHours(96),
                Ip_address = _contextAccessor.HttpContext?
                      .Connection?.RemoteIpAddress?.ToString()
                      ?? "0.0.0.0"
            });
            //var origin = _gatewayBaseUrl.TrimEnd('/');
            var origin = GetFrontendBaseUrl();
                var verificationLink = $"{origin}/call?token={encryptedToken}";
            return verificationLink;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        public async Task<bool> AddInterviewToken(UserTokenRequestViewModel user)
        {
            var userIdClaim = _contextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value;
            var userId = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
            using var conn = _dbFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("user_id", userId);
            parameters.Add("email", "");
            parameters.Add("token", user.Token);
            parameters.Add("token_data", user.Token_data);
            parameters.Add("token_type", user.Token_type);
            parameters.Add("token_expiry", user.Token_expiry);
            parameters.Add("status", user.Status.ToString());
            parameters.Add("created_by", user.User_Id);
            parameters.Add("ip_address", user.Ip_address);
            parameters.Add("out_user_token_id", dbType: DbType.Guid, direction: ParameterDirection.Output);
            await conn.ExecuteAsync(StoreProcedure.AddUserToken, parameters, commandType: CommandType.StoredProcedure);

            // Read the OUT parameter
            var newTokenId = parameters.Get<Guid>("out_user_token_id");

            return newTokenId != Guid.Empty;
        }

        public async Task<string> GetCandidateAsync(Guid interviewId)
        {
            var origin = _gatewayInterviewLinkBaseUrl.TrimEnd('/');
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{origin}/api/interview/Interview/get-by-id?Id={interviewId}"
            );

            // 🔑 INTERNAL API KEY
            request.Headers.Add("X-API-KEY", "SERVICE_KEY_123");

            var response = await _client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<Guid> CreateInterviewInvitationAsync(Guid? organizationId, Guid Interview_id, Guid Candidate_id, DateTime Interview_date, bool IsDirect)
        {
            Guid? createdBy = Guid.Empty;
            if (IsDirect == true)
            {
                var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
            }

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("_id", null);
            parameters.Add("_organization_id", organizationId);
            parameters.Add("_interview_id", Interview_id);
            parameters.Add("_candidate_id", Candidate_id);
            parameters.Add("_interview_date", Interview_date);
            parameters.Add("_start_date_time", null);
            parameters.Add("_end_date_time", null);
            parameters.Add("_updated_by", createdBy);

            // Output
            parameters.Add("_return_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.CandidateInterviewInvitationUpdate,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            // Output values
            var outOrgId = parameters.Get<Guid?>("_return_id") ?? Guid.Empty;

            // Return in ResponseViewModel wrapper
            return outOrgId;
        }

        public async Task<Guid?> GetOrganizationIdByInterviewId(Guid InterviewId) 
        {
            using var conn1 = _dbFactory.CreateConnection();

            var sql = "";
            sql = @"
                SELECT organization_id
                FROM interviews.interviews 
                WHERE id = @OrgId";
            var organization_id = await conn1.QueryFirstOrDefaultAsync<Guid?>(sql, new { OrgId = InterviewId });
            return organization_id;
        }

        private string GetFrontendBaseUrl()
        {
            var context = _contextAccessor.HttpContext;
            if (context == null)
            {
                _logger.LogWarning("HttpContext is null. Using default fallback http://localhost");
                return "http://localhost";
            }

            var request = context.Request;

            _logger.LogInformation("Detecting frontend base URL...");

            // 🔹 1️⃣ Check Origin header (BEST for frontend calls)
            string? origin = request.Headers["Origin"].ToString();
            if (!string.IsNullOrWhiteSpace(origin))
            {
                _logger.LogInformation("Frontend Origin detected: {Origin}", origin);
                return origin.TrimEnd('/');
            }

            // 🔹 2️⃣ Check Referer header (when Origin missing)
            string? referer = request.Headers["Referer"].ToString();
            if (!string.IsNullOrWhiteSpace(referer))
            {
                var uri = new Uri(referer);
                string refererUrl = $"{uri.Scheme}://{uri.Host}" + (uri.IsDefaultPort ? "" : $":{uri.Port}");

                _logger.LogInformation("Frontend Referer detected: {Referer}", refererUrl);
                return refererUrl.TrimEnd('/');
            }

            // 🔹 3️⃣ Fallback → Use API’s host (works in local/dev)
            string apiUrl = $"{request.Scheme}://{request.Host}";

            _logger.LogWarning(
                "Origin & Referer headers missing. Using API host fallback: {ApiUrl}",
                apiUrl
            );

            return apiUrl.TrimEnd('/');
        }
        #endregion
    }
}
