using AuthService.Api.Services.Interface;
using AuthService.Api.ViewModels.Request;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _iLogger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _iLogger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequestViewModel? request)
        {

            _iLogger.LogInformation("API hit: OrganizationLogin. Email={Email}", request?.Email);

            var result = await _authService.OrganizationLoginAsync(request);

            _iLogger.LogInformation("API response for Email={Email}: Success={Success}", request?.Email, result.Success);

            return StatusCode(result.StatusCode, result);
        }
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {

            _iLogger.LogInformation("API hit: OrganizationLogin. Email={Email}", request?.Email);

            var result = await _authService.ForgotPasswordAsync(request?.Email ?? string.Empty);

            _iLogger.LogInformation("API response for Email={Email}: Success={Success}", request?.Email, result.Success);

            return StatusCode(result.StatusCode, result);
        }
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            _iLogger.LogInformation("API Hit: ResetPassword. Token={Token}", request.Token);

            var result = await _authService.ResetPasswordAsync(request);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            _iLogger.LogInformation("API hit: ChangePassword for Email={Email}", request.Email);

            var result = await _authService.ChangePasswordAsync(request);

            _iLogger.LogInformation("API hit: Password Changed for Email={Email}", request.Email);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            _iLogger.LogInformation("API Hit: Logout for UserId={UserId}", request?.UserId);

            var result = await _authService.LogoutAsync(request!);

            _iLogger.LogInformation("Logout response for UserId={UserId}: Success={Success}",
                request?.UserId, result.Success);

            return StatusCode(result.StatusCode, result);
        }
    }
}
