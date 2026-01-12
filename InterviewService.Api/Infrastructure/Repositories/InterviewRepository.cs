using Dapper;

using InterviewService.Api.Helpers;
using InterviewService.Api.Helpers.EncryptionHelpers.Handlers;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.ViewModels.Request.Interview;
using InterviewService.Api.ViewModels.Response.Interview;
using InterviewService.Api.ViewModels.Response.Interview_Detail;
using Microsoft.AspNetCore.Connections;
using Npgsql;

using System.Data;
using System.Data.Common;
using System.Security.Claims;

namespace InterviewService.Api.Infrastructure.Repositories
{
    public class InterviewRepository : IInterviewRepository
    {
        private readonly ILogger<InterviewRepository> _logger;
        private readonly EncryptionHelper _encryptionHelper;
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly string _gatewayBaseUrl;

        public InterviewRepository(IDbConnectionFactory dbFactory, ILogger<InterviewRepository> logger, IConfiguration configuration, IHttpContextAccessor contextAccessor, EncryptionHelper encryptionHelper)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _contextAccessor = contextAccessor;
            _encryptionHelper = encryptionHelper;
            _configuration = configuration;
            _gatewayBaseUrl = _configuration["ImageUploadSettings:BaseUrl"]
                              ?? string.Empty;
        }

        public async Task<InterviewListResponseViewModel> GetInterviewsAsync(
            string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null, Guid? OrganizationId = null)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetInterviewList}(@p_search, @p_length, @p_page, @p_order_column, @p_order_direction, @p_is_active, @p_organization_id, @o_total_numbers, @ref)",
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

			cmd.Parameters.Add(new NpgsqlParameter("p_organization_id", NpgsqlTypes.NpgsqlDbType.Uuid)
			{
				Value = OrganizationId ?? (object)DBNull.Value
			});

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

            var interviews = conn.Query<InterviewData>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            // Map output parameters to entity
            var data = new InterviewListResponseViewModel
            {
                TotalNumbers = (int)totalParam.Value,
                InterviewData = interviews
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
 
        public async Task<InterviewByIdResponseViewModel> GetInterviewByIdAsync(Guid Id)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                  $"CALL {StoreProcedure.GetInterviewById}(@p_id, @ref_interview)",
                  (NpgsqlConnection)conn
              );

            cmd.Transaction = (NpgsqlTransaction)tran;

            cmd.Parameters.AddWithValue("p_id", Id);

            cmd.Parameters.Add(new NpgsqlParameter("ref_interview", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "interview_cursor"
            });

            cmd.ExecuteNonQuery();

            // Fetch interview
            var interview = conn
                .Query<Interview>("FETCH ALL IN \"interview_cursor\"", transaction: tran)
                .FirstOrDefault();

            tran.Commit();

            return new InterviewByIdResponseViewModel
            {
                Interview = interview,
            };
        }

        public async Task<InterviewCreateResponseViewModel> CreateInterviewAsync(InterviewCreateRequestViewModel request, string documentFile)
        {
            _logger.LogInformation("Executing InterviewCreate stored procedure for Interview Name={Name}", request.Name);

            var userIdClaim = _contextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value;
            var organizationIdClaim = _contextAccessor.HttpContext?.User?.FindFirst("organization_id")?.Value;
            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
            var organizationId = string.IsNullOrWhiteSpace(organizationIdClaim) ? (Guid?)null : Guid.Parse(organizationIdClaim);
            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_name", request.Name);
            parameters.Add("p_interviewer_id", request.Interviewer_Id);
            parameters.Add("p_interview_type_id", request.Interview_Type_Id);
            parameters.Add("p_department_id", request.Department_Id);
            parameters.Add("p_organization_id", organizationId);
            parameters.Add("p_user_id", createdBy);
            parameters.Add("p_description", request.Description);
            parameters.Add("p_work_mode_id", request.Work_Mode_Id);
            parameters.Add("p_document", documentFile);
            parameters.Add("p_experience", request.Experience);
            parameters.Add("p_no_of_question", request.No_Of_Question);
            parameters.Add("p_duration_mins", request.Duration_Mins);
            parameters.Add("p_created_by", createdBy);

            // Output
            parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.InterviewCreate,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            // Output values
            var outOrgId = parameters.Get<Guid?>("o_id") ?? Guid.Empty;

            // Map output parameters to entity
            var data = new InterviewCreateResponseViewModel
            {
                Id = outOrgId,
                Name = request.Name,
                Interviewer_Id = request.Interviewer_Id,
                Interview_Type_Id = request.Interview_Type_Id,
                Department_Id = request.Department_Id,
                Organization_Id = organizationId,
                User_Id = createdBy,
                Description = request.Description,
                Work_Mode_Id = request.Work_Mode_Id,
				Experience = request.Experience,
                Document = documentFile,
                No_Of_Question = request.No_Of_Question,
                Duration_Mins = request.Duration_Mins
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        public async Task DeleteInterviewAsync(InterviewDeleteRequestViewModel request)
        {
            _logger.LogInformation("Executing DeleteInterviewAsync stored procedure for Id={Id}", request.Id);
            var interviewIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(interviewIdClaim) ? (Guid?)null : Guid.Parse(interviewIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_id", request.Id);
            parameters.Add("p_updated_by", updatedBy);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.InterviewDelete,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<InterviewInitResponseViewModel> GetInterviewInitAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            try
            {
                var httpUser = _contextAccessor.HttpContext?.User;

                if (httpUser == null)
                    throw new UnauthorizedAccessException("User not logged in");

                // 🔹 Get OrganizationId from claims
                var orgIdClaim = httpUser.FindFirst("organization_id")?.Value;

                if (string.IsNullOrWhiteSpace(orgIdClaim))
                    throw new UnauthorizedAccessException("Organization not found in token");

                var organizationId = Guid.Parse(orgIdClaim);

                await using var cmd = new NpgsqlCommand(
                    $"CALL {StoreProcedure.GetInterviewInit}(@p_organization_id, @ref_interviewer,@ref_interview_type,@ref_department, @ref_api_key, @ref_work_mode)",
                    (NpgsqlConnection)conn
                );

                cmd.Transaction = (NpgsqlTransaction)tran;

                // 🔹 Input parameter
                cmd.Parameters.AddWithValue("p_organization_id", organizationId);

                // 🔹 Interviewer cursor
                cmd.Parameters.Add(new NpgsqlParameter("ref_interviewer", NpgsqlTypes.NpgsqlDbType.Refcursor)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = "interviewer_cursor"
                });
                cmd.Parameters.Add(new NpgsqlParameter("ref_interview_type", NpgsqlTypes.NpgsqlDbType.Refcursor)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = "interview_type_cursor"
                });

                cmd.Parameters.Add(new NpgsqlParameter("ref_department", NpgsqlTypes.NpgsqlDbType.Refcursor)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = "department_cursor"
                });

                // 🔹 API key cursor
                cmd.Parameters.Add(new NpgsqlParameter("ref_api_key", NpgsqlTypes.NpgsqlDbType.Refcursor)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = "apikey_cursor"
                });

				// 🔹 Work mode cursor
				cmd.Parameters.Add(new NpgsqlParameter("ref_work_mode", NpgsqlTypes.NpgsqlDbType.Refcursor)
				{
					Direction = ParameterDirection.InputOutput,
					Value = "work_mode_cursor"
				});

				await cmd.ExecuteNonQueryAsync();

                // 🔹 Fetch interviewer list
                var interviewerList = conn
                    .Query<IntervieweInitData>(
                        "FETCH ALL IN \"interviewer_cursor\"",
                        transaction: tran
                    )
                    .ToList();
                var interviewTypeList = conn.Query<InterviewTypeInitData>(
                      "FETCH ALL IN \"interview_type_cursor\"",
                      transaction: tran
                  ).ToList();

                var departmentList = conn
                .Query<DepartmentInitData>(
                    "FETCH ALL IN \"department_cursor\"",
                    transaction: tran
                ).ToList();

                // 🔹 Fetch API key
                var apiKey = conn
                    .QueryFirstOrDefault<string>(
                        "FETCH ALL IN \"apikey_cursor\"",
                        transaction: tran
                    );

				var workModeList = conn.Query<WorkModeInitData>(
					 "FETCH ALL IN \"work_mode_cursor\"",
					 transaction: tran
				 ).ToList();

				tran.Commit();

                return new InterviewInitResponseViewModel
                {
                    InterviewerList = interviewerList,
                    InterviewTypeList = interviewTypeList,
                    DepartmentList = departmentList,
                    WorkModeList = workModeList,
					Generate_question_key = apiKey ?? string.Empty
                };
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<InterviewUpdateResponseViewModel> UpdateInterviewAsync(InterviewUpdateRequestViewModel request, string documentFile)
        {
            using var conn = _dbFactory.CreateConnection();
            var userIdClaim = _contextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(userIdClaim)
                ? (Guid?)null
                : Guid.Parse(userIdClaim);

            try
            {
                // 🔹 1. Update Interview (parent)
                var interviewParams = new DynamicParameters();

                interviewParams.Add("p_id", request.Id);
                interviewParams.Add("p_name", request.Name);
                interviewParams.Add("p_interviewer_id", request.Interviewer_Id);
                interviewParams.Add("p_interview_type_id", request.Interview_Type_Id);
                interviewParams.Add("p_department_id", request.Department_Id);
                interviewParams.Add("p_description", request.Description);
                interviewParams.Add("p_experience", request.Experience);
                interviewParams.Add("p_work_mode_id", request.Work_Mode_Id);
                interviewParams.Add("p_document", documentFile);
                interviewParams.Add("p_job_status", request.Job_status);
                interviewParams.Add("p_no_of_question", request.No_Of_Question);
                interviewParams.Add("p_duration_mins", request.Duration_Mins);
                interviewParams.Add("p_updated_by", updatedBy);
                interviewParams.Add(
                      "o_id",
                      dbType: DbType.Guid,
                      direction: ParameterDirection.Output
                  );
                await conn.ExecuteAsync(
                        StoreProcedure.InterviewUpdate,
                        interviewParams,
                        commandType: CommandType.StoredProcedure
                    );
                Guid? interviewId = interviewParams.Get<Guid?>("o_id");
                var data = new InterviewUpdateResponseViewModel
                {
                    Id = request.Id,
                    Name = request.Name,
                    Interviewer_Id = request.Interviewer_Id,
                    Interview_Type_Id = request.Interview_Type_Id,
                    Department_Id = request.Department_Id,
                    Description = request.Description,
                    Work_Mode_Id = request.Work_Mode_Id,
					Experience = request.Experience,
                    Document = documentFile,
                    Job_status = request.Job_status,
                    No_Of_Question = request.No_Of_Question,
                    Duration_Mins = request.Duration_Mins,
                };
                return data;
            }
            catch
            {
                throw;
            }
        }
  
        public async Task<string> CreateInterviewTokenAsync(InterviewTokenRequestViewModel request)
        {
            try
            {
                var interviewer = await GetInterviewByIdAsync(request.Interview_id);
                var interviewerId = interviewer.Interview?.Interviewer_Id;
                var organizationId = interviewer.Interview?.Organization_Id;
                var payload = new
                {
                    InterviewId = request.Interview_id,
                    CandidateId = request.Candidate_id,
                    InterviewDate = request.Interview_date
                };
                var payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                var encryptedToken = _encryptionHelper.EncryptForReact(payloadJson);
                await AddInterviewToken(new UserTokenRequestViewModel
                {
                    Token = encryptedToken,
                    Token_data = payloadJson,
                    Token_type = "InterviewInvitation",
                    Status = 1,
                    Token_expiry = payload.InterviewDate.AddHours(96),
                    Ip_address = _contextAccessor.HttpContext?
                          .Connection?.RemoteIpAddress?.ToString()
                          ?? "0.0.0.0"
                });
                var origin = _gatewayBaseUrl.TrimEnd('/');

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

        public async Task<(int StatusCode, string Message, string? TokenData)> VerifyInterviewTokenAsync(string token)
        {
            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("p_token", token);
            parameters.Add("p_status_code", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("p_result_message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);
            parameters.Add("p_token_data", dbType: DbType.String, direction: ParameterDirection.Output);

            await conn.ExecuteAsync(
                StoreProcedure.VerifyInterviewToken,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return (
                parameters.Get<int>("p_status_code"),
                parameters.Get<string>("p_result_message"),
                parameters.Get<string?>("p_token_data")
            );
        }
 
        public async Task<string?> GetCandidateEmailByIdAsync(Guid candidateId)
        {
            using var conn = _dbFactory.CreateConnection();

            var sql = @"
                    SELECT
                        email
                    FROM master.candidate_detail
                    WHERE id = @candidateId
                      AND is_delete = false;
                ";

            return await conn.QueryFirstOrDefaultAsync<string>(
                sql,
                new { candidateId }
            );
        }

        public async Task<InterviewOverviewResponseViewModel> GetInterviewOverviewAsync(Guid interviewId, Guid candidateId, Guid interviewerId)
        {
            try
            {
                using var conn = _dbFactory.CreateConnection();
                conn.Open();

                using var tran = conn.BeginTransaction();

                await using var cmd = new NpgsqlCommand(
                    $"CALL {StoreProcedure.GetInterviewOverview}(@p_interview_id, @p_candidate_id, @p_interviewer_id, @ref)",
                    (NpgsqlConnection)conn
                );
                cmd.Transaction = (NpgsqlTransaction)tran;

                cmd.Parameters.AddWithValue("p_interview_id", interviewId);
                cmd.Parameters.AddWithValue("p_candidate_id", candidateId);
                cmd.Parameters.AddWithValue("p_interviewer_id", interviewerId);

                var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = "my_cursor"
                };
                cmd.Parameters.Add(cursorParam);

                await cmd.ExecuteNonQueryAsync();

                var result = conn.QueryFirstOrDefault<dynamic>(
                    "FETCH ALL IN \"my_cursor\"",
                    transaction: tran
                );

                tran.Commit();

                if (result == null)
                    return null;

                return new InterviewOverviewResponseViewModel
                {
                    Candidate = new CandidateInfo
                    {
                        Candidate_id = result.candidate_id,
                        Candidate_name = result.candidate_name,
                        Candidate_avatar = result.candidate_avatar
                    },
                    Interview = new InterviewInfo
                    {
                        Interview_id = result.interview_id,
                        Interview_name = result.interview_name,
                        Description = result.description,
                        Duration_mins = result.duration_mins
                    },
                    Interviewer = new InterviewerInfo
                    {
                        Interviewer_id = result.interviewer_id,
                        Interviewer_name = result.interviewer_name,
                        Interviewer_avatar = result.interviewer_avatar
                    }
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<CandidateInterviewInvitationViewModel?> GetCandidateInterviewInvitationAsync(Guid id)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetCandidateInterviewInvitation}(@p_id, @ref_invitation)",
                (NpgsqlConnection)conn
            );

            cmd.Transaction = (NpgsqlTransaction)tran;

            cmd.Parameters.AddWithValue("p_id", id);

            cmd.Parameters.Add(new NpgsqlParameter("ref_invitation", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "invitation_cursor"
            });

            cmd.ExecuteNonQuery();

            var invitation = conn
                .Query<CandidateInterviewInvitationViewModel>(
                    "FETCH ALL IN \"invitation_cursor\"",
                    transaction: tran
                )
                .FirstOrDefault();

            tran.Commit();

            return invitation;
        }
        public async Task<int> UpdateCallIdAsync(
            Guid candidateInterviewInvitationId,
            string callId)
                {
                    using var conn = _dbFactory.CreateConnection();

                    const string sql = @"
                UPDATE interviews.candidate_interview_invitation
                SET call_id = @callId
                WHERE id = @candidateInterviewInvitationId;
            ";

            return await conn.ExecuteAsync(sql, new
            {
                candidateInterviewInvitationId,
                callId
            });
        }



    }
}