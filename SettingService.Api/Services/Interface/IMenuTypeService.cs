using Common.Services.Helper;

using Microsoft.AspNetCore.Mvc;

using SettingService.Api.ViewModels.Request.Menu_type;
using SettingService.Api.ViewModels.Response.Menu_type;

namespace SettingService.Api.Services.Interface
{
    public interface IMenuTypeService
    {
        #region Menu Type
        Task<IActionResult> CreateUpdateMenuTypeAsync(MenuTypeCreateUpdateRequestViewModel model);
        Task<IActionResult> GetMenuTypeListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null);
        Task<IActionResult> GetMenuTypeByIdAsync(Guid? Id);
        Task<IActionResult> DeleteMenuTypeAsync(MenuTypeDeleteRequestViewModel request);
        #endregion

        #region Menu Permission
        Task<ApiResponse<MenuPermissionDeleteResponseViewModel>> MenuPermissionDeleteAsync(MenuPermissionDeleteRequestViewModel model);
        Task<ApiResponse<MenuPermissionCreateUpdateResponseViewModel>> CreateUpdateMenuPermissionAsync(MenuPermissionCreateUpdateRequestViewModel model);
        Task<ApiResponse<MenuPermissionListResponseViewModel>> MenuPermissionListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection);
        Task<ApiResponse<MenuPermissionResponseViewModel>> MenuPermissionByIdAsync(Guid? id);
        #endregion

    }
}
