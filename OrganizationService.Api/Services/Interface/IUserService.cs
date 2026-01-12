using OrganizationService.Api.ViewModels.Request.User;
using OrganizationService.Api.ViewModels.Response;
using OrganizationService.Api.ViewModels.Response.User;

namespace OrganizationService.Api.Services.Interface
{
    public interface IUserService
    {
        Task<ApiResponse<UserCreateUpdateResponseViewModel>> CreateUpdateUserAsync(UserCreateUpdateRequestViewModel model);
        Task<ApiResponse<UserListResponseViewModel>> GetUserListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", string role = "", bool? isActive = null);
        Task<ApiResponse<UserListResponseViewModel>> DeleteUserAsync(UserDeleteRequestViewModel request);
        Task<ApiResponse<UserData>> GetUserProfile(string id);
        Task<ApiResponse<UserData>> GetUserByIdAsync(Guid? Id);
    }
}
