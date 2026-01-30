using AuthService.Api.Helpers;
using AuthService.Api.Infrastructure.Interface;
using AuthService.Api.ViewModels.Request;
using AuthService.Api.ViewModels.Response;

using Common.Services.Helper;
using Common.Services.ViewModels;

using Dapper;

using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Data;



namespace AuthService.Api.Infrastructure.Repositories
{
	public class AuthRepository : IAuthRepository
	{
		#region Repository
		private readonly ILogger<AuthRepository> _logger;
		private readonly IDbConnectionFactory _dbFactory;
		private readonly JwtTokenHelper _jwtTokenHelper;
		private readonly JwtSettings _jwtConfig;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly HttpClient _client;
		private readonly IConfiguration _configuration;
		private readonly string _gatewayOrganizationSubscriptionBaseUrl;

		public AuthRepository(IDbConnectionFactory dbFactory, ILogger<AuthRepository> logger, JwtTokenHelper jwtToken, IHttpContextAccessor httpContextAccessor, IOptions<JwtSettings> jwtSettings, HttpClient client, IConfiguration configuration)
		{
			_dbFactory = dbFactory;
			_logger = logger;
			_jwtTokenHelper = jwtToken;
			_jwtConfig = jwtSettings.Value;
			_httpContextAccessor = httpContextAccessor;
			_client = client;
			_configuration = configuration;
			_gatewayOrganizationSubscriptionBaseUrl = _configuration["OrganizationSubscription:BaseUrl"] ?? string.Empty;
		}

		public async Task<AuthResponseViewModel> OrganizationLoginAsync(AuthRequestViewModel request)
		{
			_logger.LogInformation("Executing OrganizationLogin stored procedure for Email={Email}", request.Email);


			using var conn = _dbFactory.CreateConnection();

			var parameters = new DynamicParameters();

			// Input
			parameters.Add("p_email", request.Email);

            // Output
            parameters.Add("user_id", dbType: DbType.Guid, direction: ParameterDirection.Output);
            parameters.Add("org_id", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("org_name", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);
            parameters.Add("role_name", dbType: DbType.String, size: 100, direction: ParameterDirection.Output);
            parameters.Add("org_website", dbType: DbType.String, size: 200, direction: ParameterDirection.Output); // <-- required
            parameters.Add("org_hash_password", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);
            parameters.Add("org_token", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);
            parameters.Add("full_name", dbType: DbType.String, size: 300, direction: ParameterDirection.Output);


            // Execute procedure
            await conn.ExecuteAsync(
					StoreProcedure.OrganizationLogin,
					parameters,
					commandType: CommandType.StoredProcedure
					);

			// Map output parameters to entity
			var data = new AuthResponseViewModel
			{
				UserId = parameters.Get<Guid?>("user_id"),
                OrganizationId = parameters.Get<int?>("org_id"),
                OrganizationName = parameters.Get<string?>("org_name"),
				RoleName = parameters.Get<string?>("role_name"),
				HashPassword = parameters.Get<string?>("org_hash_password") ?? "",
				Email = request.Email,
				FullName = parameters.Get<string?>("full_name") ?? ""
			};

			if (data.UserId is not null)
			{
				var isSuperAdmin = string.Equals(data.RoleName, "Super Admin", StringComparison.OrdinalIgnoreCase);
				//Jwt Token Generation
				var token = _jwtTokenHelper.GenerateToken(
				  userId: data.UserId.Value.ToString(),
				  email: data.Email,
				  role: data.RoleName ?? "",
				  organizationId: isSuperAdmin ? null : data.OrganizationId?.ToString(),
				  jwtSettings: _jwtConfig
			  );

				if (string.IsNullOrWhiteSpace(token))
				{
					throw new Exception("JWT generation failed.");
				}
				var payload = new { UserId = data.UserId, Email = data.Email };
				var payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
				await AddUserToken(new UserTokenRequestViewModel
				{
					User_Id = data.UserId,
					Email = data.Email,
					Token = token,
					Token_data = payloadJson,
					Token_type = "UserLogin",
					Token_expiry = DateTime.UtcNow.AddHours(24),
					Ip_address = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0"
				});
				// Return in ResponseViewModel wrapper
				data.Token = token;
			}
			return data;
		}
        public async Task<bool> AddUserToken(UserTokenRequestViewModel user)
        {
            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("p_user_id", user.User_Id);
            parameters.Add("p_email", user.Email);
            parameters.Add("p_token", user.Token);
            parameters.Add("p_token_data", user.Token_data);
            parameters.Add("p_token_type", user.Token_type);
            parameters.Add("p_expired_at", user.Token_expiry);
            parameters.Add("p_status", "ACTIVE"); // or whatever status you want
            parameters.Add("p_created_by", user.User_Id);
            parameters.Add("p_ip_address", user.Ip_address);

            parameters.Add(
                "out_user_token_id",
                dbType: DbType.Guid,
                direction: ParameterDirection.Output
            );

            await conn.ExecuteAsync(
                StoreProcedure.AddUserToken,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return parameters.Get<Guid?>("out_user_token_id") != null;
        }

        public async Task<UserResponse?> GetUserByEmailAsync(string email)
		{

			using var conn = _dbFactory.CreateConnection();

			_logger.LogInformation("Fetching user by email: {Email}", email);

			const string query = @"
            SELECT 
                id AS user_id,
                email,
                first_name,
                last_name,
                hash_password
            FROM master.""user""
            WHERE email = @Email
            LIMIT 1;
        ";


			var user = await conn.QueryFirstOrDefaultAsync<UserResponse>(query, new { Email = email });

			if (user == null)
			{
				_logger.LogWarning("User not found for email: {Email}", email);
			}
			else
			{
				_logger.LogInformation("User found: {Email}", email);
			}

			return user;
		}

		public async Task<bool> SaveResetTokenAsync(UserResponse userInfo, string token)
		{
			using var conn = _dbFactory.CreateConnection();

			var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
					 ?? "0.0.0.0";

			await conn.ExecuteAsync(
					"SELECT master.fn_save_reset_token(@p_user_id, @p_email, @p_token, @p_ip_address)",
					new
					{
						p_user_id = userInfo.user_id,
						p_email = userInfo.email,
						p_token = token,
						p_ip_address = ip
					}
				);

			return true;
		}

		public async Task<UserTokenResponse?> GetUserByResetTokenAsync(string token)
		{
			_logger.LogInformation("Fetching user by reset token: {Token}", token);

			using var conn = _dbFactory.CreateConnection();

			const string query = @"
                                    SELECT 
                                        user_id, 
                                        email,
                                        expired_at
                                    FROM master.user_tokens
                                    WHERE token = @Token 
                                      AND is_used = false";

			var user = await conn.QueryFirstOrDefaultAsync<UserTokenResponse>(query, new { Token = token });

			if (user == null)
			{
				_logger.LogWarning("No user found for reset token: {Token}", token);
			}
			else
			{
				_logger.LogInformation("User found for reset token. UserId: {UserId}, Email: {Email}",
									   user.user_id, user.email);
			}

			return user;
		}

		public async Task<bool> UpdatePasswordAsync(string token, string hashedPassword, UserTokenResponse user)
		{
			using var conn = _dbFactory.CreateConnection();

			_logger.LogInformation("Starting password update for UserId: {UserId}, Email: {Email}",
								   user.user_id, user.email);

			// 1️⃣ Mark token as used
			const string updateTokenQuery = @"
                    UPDATE master.user_tokens
                    SET 
                        is_used = true,
                        expired_at = NULL,
                        updated_date = NOW()
                    WHERE token = @Token";

			var affectedTokenRows = await conn.ExecuteAsync(updateTokenQuery, new { Token = token });

			_logger.LogInformation("Token update affected rows: {Rows}", affectedTokenRows);


			// 2️⃣ Update user password
			const string updatePasswordQuery = @"
            UPDATE master.user
            SET hash_password = @HashedPassword
            WHERE id = @UserId";

			var affectedUserRows = await conn.ExecuteAsync(updatePasswordQuery, new
			{
				UserId = user.user_id,
				HashedPassword = hashedPassword
			});


			_logger.LogInformation("Password updated for UserId: {UserId}, RowsAffected: {Rows}",
									user.user_id, affectedUserRows);

			return affectedUserRows > 0;

		}

		public async Task<bool> ChangePasswordAsync(Guid userId, string hashedPassword)
		{
			_logger.LogInformation("ChangePasswordAsync started for UserId={UserId}", userId);

			using var conn = _dbFactory.CreateConnection();

			const string query = @"CALL master.sp_change_password(@p_user_id, @p_new_password)";

			_logger.LogInformation("Executing stored procedure sp_change_password for UserId={UserId}", userId);

			await conn.ExecuteAsync(query, new
			{
				p_user_id = userId,
				p_new_password = hashedPassword
			});

			return true;
		}

		public async Task<bool> LogoutAsync(Guid? userId)
		{

			using var conn = _dbFactory.CreateConnection();

			_logger.LogInformation("Executing logout for UserId={UserId}", userId);

			const string query = @"
            UPDATE master.user_tokens
            SET is_used = TRUE,
                updated_by = @UserId,
                token_type = 'UserLogout',
                is_delete = TRUE,
                updated_date = NOW()
            WHERE user_id = @UserId
              AND token_type='UserLogin'
              AND is_delete = FALSE
              AND is_used = FALSE;
        ";

			var affected = await conn.ExecuteAsync(query, new { UserId = userId });

			if (affected > 0)
			{
				_logger.LogInformation("Logout successful — token invalidated for UserId={UserId}", userId);
				return true;
			}

			_logger.LogWarning("Logout failed — No active token found for UserId={UserId}", userId);
			return false;
		}

		public async Task<List<Subscription>> GetSubscriptionByOrganizationIdAsync(int? OrganizationId)
		{
			var origin = _gatewayOrganizationSubscriptionBaseUrl.TrimEnd('/');
			var request = new HttpRequestMessage(
				HttpMethod.Get,
				$"{origin}/api/setting/Subscription/getByOrganizationId?OrganizationId={OrganizationId}"
			);

			var response = await _client.SendAsync(request);

			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();

			var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Subscription>>(json);
			return apiResponse?.Data != null
					? new List<Subscription> { apiResponse.Data }
					: new List<Subscription>();
		}
		#endregion
	}
}
