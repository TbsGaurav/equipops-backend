using SettingService.Api.ViewModels.Request;
using SettingService.Api.ViewModels.Response.UserAccessRole;
using SettingService.Api.ViewModels.Response.UserRole;

namespace SettingService.Api.Infrastructure.Interface
{
    public interface IUserRoleRepository
    {
        #region User Role
        Task<UserRoleResponseViewModel> UserRoleCreateAsync(UserRoleRequestViewModel request);
        Task<UserRoleListResponseViewModel> UserRoleListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection);
        Task<UserRoleDeleteResponseViewModel> UserRoleDeleteAsync(UserRoleDeleteRequestViewModel request);
        Task<UserRoleResponseViewModel> UserRoleByIdAsync(Guid? id);
        #endregion

        #region User Access Role
        Task<UserAccessRoleCreateUpdateResponseViewModel> UserAccessRoleCreateAsync(UserAccessRoleRequestViewModel request);
        Task<UserAccessRoleListResponseViewModel> UserAccessRoleListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection);
        Task<UserAccessRoleDeleteResponseViewModel> UserAccessRoleDeleteAsync(UserAccessRoleDeleteRequestViewModel request);
        Task<UserAccessRoleResponseViewModel> UserAccessRoleByIdAsync(Guid? id);
        #endregion
    }
}