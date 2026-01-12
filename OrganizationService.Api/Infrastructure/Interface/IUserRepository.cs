using OrganizationService.Api.ViewModels.Request.User;
using OrganizationService.Api.ViewModels.Response.User;

namespace OrganizationService.Api.Infrastructure.Interface
{
    public interface IUserRepository
    {
        Task<UserCreateUpdateResponseViewModel> CreateUpdateUserAsync(UserCreateUpdateRequestViewModel request);
        Task<UserListResponseViewModel> GetUserListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", string role = "", bool? isActive = null);
        Task DeleteUserAsync(UserDeleteRequestViewModel request);
        Task<UserData> GetUserProfile(string id);
        Task<UserData> GetUserByIdAsync(Guid? Id);
    }
}
