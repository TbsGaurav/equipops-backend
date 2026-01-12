using AuthService.Api.ViewModels.Request;
using AuthService.Api.ViewModels.Response;

namespace AuthService.Api.Infrastructure.Interface
{
	public interface IAuthRepository
	{
		Task<AuthResponseViewModel> OrganizationLoginAsync(AuthRequestViewModel request);
		Task<UserResponse?> GetUserByEmailAsync(string email);
		Task<bool> SaveResetTokenAsync(UserResponse userInfo, string resetToken);
		Task<UserTokenResponse?> GetUserByResetTokenAsync(string token);
		Task<bool> UpdatePasswordAsync(string token, string hashedPassword, UserTokenResponse user);
		Task<bool> ChangePasswordAsync(Guid userId, string hashedPassword);
		Task<bool> LogoutAsync(Guid? userId);
		Task<List<Subscription>> GetSubscriptionByOrganizationIdAsync(Guid? OrganizationId);
	}
}
