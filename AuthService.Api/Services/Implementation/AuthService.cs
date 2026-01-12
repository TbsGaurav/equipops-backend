using AuthService.Api.Infrastructure.Interface;
using AuthService.Api.Services.Interface;
using AuthService.Api.ViewModels.Request;
using AuthService.Api.ViewModels.Response;

using Common.Services.Helper;
using Common.Services.Helpers;
using Common.Services.Services.Interface;

using System.Security.Cryptography;

namespace AuthService.Api.Services.Implementation
{
	public class AuthService : IAuthService
	{
		#region Init AuthService
		private readonly IAuthRepository _authRepository;
		private readonly IEmailService _emailService;
		private readonly ILogger<AuthService> _logger;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IMenuPermissionRepository _menuPermissionRepository;
		private readonly string _gatewayBaseUrl;
		private readonly IConfiguration _configuration;

		public AuthService(IAuthRepository authRepository, ILogger<AuthService> logger, IEmailService emailService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IMenuPermissionRepository menuPermissionRepository)
		{
			_authRepository = authRepository;
			_logger = logger;
			_emailService = emailService;
			_httpContextAccessor = httpContextAccessor;
			_configuration = configuration;
			_gatewayBaseUrl = _configuration["ImageUploadSettings:BaseUrl"]
				  ?? string.Empty;
			_menuPermissionRepository = menuPermissionRepository;
		}
		#endregion

		#region Login
		public async Task<ApiResponse<AuthResponseViewModel>> OrganizationLoginAsync(AuthRequestViewModel? model)
		{
			_logger.LogInformation("AuthService: OrganizationLoginAsync START. Email={Email}", model?.Email);

			// Validate Input
			if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
			{
				return new ApiResponse<AuthResponseViewModel>(
					"Email or password cannot be empty.",
					(int)ApiStatusCode.BadRequest
				);
			}

			var data = await _authRepository.OrganizationLoginAsync(model);

			if (data.UserId is null)
			{
				return new ApiResponse<AuthResponseViewModel>(
					"Invalid email or password.",
					(int)ApiStatusCode.Unauthorized
				);
			}

			// Validate Password
			bool validPassword = PasswordHelper.VerifyPassword(model.Password, data.HashPassword);

			if (!validPassword)
			{
				return new ApiResponse<AuthResponseViewModel>(
					"Incorrect password.",
					(int)ApiStatusCode.Unauthorized
				);
			}
			data.PhotoUrl = GetProfilePhotoUrl(data.PhotoUrl);
			data.NameInit = GetInitials(data.FullName);

			string[] menuPermissionList = await _menuPermissionRepository.GetPermissionList(data.UserId);
			data.menuPermissions = menuPermissionList;

			var subscription = await _authRepository.GetSubscriptionByOrganizationIdAsync(data.OrganizationId);
			data.subscription = subscription;
			
			// Login Successful
			return new ApiResponse<AuthResponseViewModel>(
				data,
				"Login successful.",
				(int)ApiStatusCode.Success
			);
		}

		#endregion

		#region Forgot Password

		public async Task<ApiResponse<ForgotPasswordResponse>> ForgotPasswordAsync(string email)
		{
			_logger.LogInformation("ForgotPassword initiated for Email={Email}", email);

			// 🔹 1. Validate user exists
			var user = await _authRepository.GetUserByEmailAsync(email);
			if (user is null)
			{
				_logger.LogWarning("ForgotPassword failed. User not found for Email={Email}", email);

				return new ApiResponse<ForgotPasswordResponse>(
					"User not found.",
					(int)ApiStatusCode.NotFound
				);
			}

			// 🔹 2. Generate token
			string resetToken = GenerateEmailToken(email);

			// 🔹 3. Save token in DB
			bool tokenSave = await _authRepository.SaveResetTokenAsync(user, resetToken);
			if (!tokenSave)
			{
				_logger.LogError("Failed to save reset token in DB for Email={Email}", email);

				return new ApiResponse<ForgotPasswordResponse>(
					"Something went wrong while generating reset link.",
					(int)ApiStatusCode.ServerError
				);
			}

			// 🔹 4. Build Reset URL
			string baseUrl = GetFrontendBaseUrl();
			string resetUrl = $"{baseUrl}/reset-password/{resetToken}";

			_logger.LogInformation("Sending password reset email to {Email}", email);

			// 🔹 5. Send email
			await _emailService.SendForgotPasswordEmailAsync(email, user.full_name, resetUrl);

			return new ApiResponse<ForgotPasswordResponse>()
			{
				Message = "Password reset link sent to your email.",
				Success = true,
				StatusCode = (int)ApiStatusCode.Success
			};
		}

		private string GenerateEmailToken(string email)
		{

			_logger.LogInformation("Generating token for {Email}", email);

			return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLower();
		}

		private string GetFrontendBaseUrl()
		{
			var context = _httpContextAccessor.HttpContext;
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

		#region ResetPassword
		public async Task<ApiResponse<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest request)
		{
			_logger.LogInformation("ResetPassword request received. Token={Token}", request.Token);

			// 🔹 1. Validate token & user
			var user = await _authRepository.GetUserByResetTokenAsync(request.Token);

			if (user == null)
			{
				_logger.LogWarning("ResetPassword failed. Invalid or expired token. Token={Token}", request.Token);

				return new ApiResponse<ResetPasswordResponse>(
					"Invalid or expired token.",
					(int)ApiStatusCode.NotFound
				);
			}

			// 🔹 2. Check token expiry
			if (user.expired_at < DateTime.UtcNow)
			{
				_logger.LogWarning("ResetPassword failed. Token expired for User={User}, ExpiredAt={ExpiredAt}",
					user.email, user.expired_at);

				return new ApiResponse<ResetPasswordResponse>(
					"Reset token has expired.",
					(int)ApiStatusCode.Unauthorized        // 410 Token expired
				);
			}

			// 🔹 3. Hash new password
			string hashedPassword = PasswordHelper.HashPassword(request.NewPassword);

			// 🔹 4. Update password
			bool isUpdated = await _authRepository.UpdatePasswordAsync(request.Token, hashedPassword, user);

			if (!isUpdated)
			{
				_logger.LogError("Password reset failed during DB update for User={User}", user.email);

				return new ApiResponse<ResetPasswordResponse>(
					"Something went wrong during password reset.",
					(int)ApiStatusCode.ServerError
				);
			}

			// 🔹 5. Success
			_logger.LogInformation("Password reset successfully for User={User}", user.email);

			return new ApiResponse<ResetPasswordResponse>(
				"Password reset successfully.",
				(int)ApiStatusCode.Success
			);
		}
		#endregion

		#region ChangePassword
		public async Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request)
		{
			_logger.LogInformation("ChangePassword request for Email={Email}", request.Email);

			// 🔹 1. Check user exists
			var user = await _authRepository.GetUserByEmailAsync(request.Email);
			if (user is null)
			{
				_logger.LogWarning("ChangePassword failed: User not found. Email={Email}", request.Email);

				return new ApiResponse<bool>(
					"User not found.",
					(int)ApiStatusCode.NotFound
				);
			}

			// 🔹 2. Validate old password
			if (!PasswordHelper.VerifyPassword(request.OldPassword, user.hash_password))
			{
				_logger.LogWarning("ChangePassword failed: Old password incorrect for Email={Email}", request.Email);

				return new ApiResponse<bool>(
					"Old password is incorrect.",
					(int)ApiStatusCode.Unauthorized
				);
			}

			// 🔹 3. Hash new password
			string newHashedPassword = PasswordHelper.HashPassword(request.NewPassword);

			// 🔹 4. Update password in DB
			bool isUpdated = await _authRepository.ChangePasswordAsync(user.user_id, newHashedPassword);

			if (!isUpdated)
			{
				_logger.LogError("ChangePassword failed during DB update for Email={Email}", request.Email);

				return new ApiResponse<bool>(
					"Failed to update password.",
					(int)ApiStatusCode.ServerError
				);
			}

			_logger.LogInformation("Password changed successfully for Email={Email}", request.Email);

			// 🔹 5. Success Response
			return new ApiResponse<bool>(
				true,
				"Password changed successfully.",
				(int)ApiStatusCode.Success
			);
		}
		#endregion

		#region Logout
		public async Task<ApiResponse<string>> LogoutAsync(LogoutRequest request)
		{
			_logger.LogInformation("Logout request received for UserId={UserId}", request.UserId);


			// 🔹 1. Logout repository call
			bool result = await _authRepository.LogoutAsync(request.UserId);

			if (result)
			{
				_logger.LogInformation("Logout successful for UserId={UserId}", request.UserId);

				return new ApiResponse<string>(
					"Successfully logged out.",
					(int)ApiStatusCode.Success
				);
			}

			_logger.LogWarning("Logout failed for UserId={UserId}. No active session found.", request.UserId);

			return new ApiResponse<string>(
				"Logout failed. No active session found.",
				(int)ApiStatusCode.NotFound
			);
		}
		#endregion

		private string GetProfilePhotoUrl(string? photo_url)
		{
			if (string.IsNullOrWhiteSpace(photo_url))
			{
				_logger.LogDebug("GetProfilePhotoUrl: Empty photo_url");
				return string.Empty;
			}

			var publicPath = $"org/{photo_url}".Replace("\\", "/");
			var fullUrl = $"{_gatewayBaseUrl.TrimEnd('/')}/{publicPath}";

			_logger.LogDebug(
				"Resolved profile photo URL | Input={PhotoUrl} | Output={FullUrl}",
				photo_url,
				fullUrl
			);

			return fullUrl;
		}

		public static string GetInitials(string fullName)
		{
			if (string.IsNullOrWhiteSpace(fullName))
				return string.Empty;

			var parts = fullName
				.Trim()
				.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length == 1)
				return parts[0].Substring(0, 1).ToUpper();

			return string.Concat(
				parts[0][0],
				parts[parts.Length - 1][0]
			).ToUpper();
		}

	}
}
