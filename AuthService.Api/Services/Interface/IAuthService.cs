using AuthService.Api.ViewModels.Request;
using AuthService.Api.ViewModels.Response;

using Common.Services.Helper;

namespace AuthService.Api.Services.Interface
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseViewModel>> OrganizationLoginAsync(AuthRequestViewModel? model);
        Task<ApiResponse<ForgotPasswordResponse>> ForgotPasswordAsync(string email);
        Task<ApiResponse<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request);

        Task<ApiResponse<string>> LogoutAsync(LogoutRequest request);
    }
}
