using Common.Services.Helper;

using SettingService.Api.ViewModels.Request;
using SettingService.Api.ViewModels.Response.UserAccessRole;
using SettingService.Api.ViewModels.Response.UserRole;

namespace SettingService.Api.Services.Interface
{
    public interface IUserRoleService
    {
        #region User Role
        Task<ApiResponse<UserRoleResponseViewModel>> UserRoleCreateAsync(UserRoleRequestViewModel model);
        Task<ApiResponse<UserRoleListResponseViewModel>> UserRoleListAsync(string? search, bool? Is_Active, int length, int page, string orderColumn, string orderDirection);
        Task<ApiResponse<UserRoleDeleteResponseViewModel>> UserRoleDeleteAsync(UserRoleDeleteRequestViewModel model);
        Task<ApiResponse<UserRoleResponseViewModel>> UserRoleByIdAsync(Guid? id);
        #endregion

        #region User Access Role
        Task<ApiResponse<UserAccessRoleCreateUpdateResponseViewModel>> UserAccessRoleCreateAsync(UserAccessRoleRequestViewModel model);
        Task<ApiResponse<UserAccessRoleListResponseViewModel>> UserAccessRoleListAsync(string? search, bool? Is_Active, int length, int page, string orderColumn, string orderDirection);
        Task<ApiResponse<UserAccessRoleResponseViewModel>> UserAccessRoleByIdAsync(Guid? id);
        Task<ApiResponse<UserAccessRoleDeleteResponseViewModel>> UserAccessRoleDeleteAsync(UserAccessRoleDeleteRequestViewModel model);
        #endregion
    }
}
