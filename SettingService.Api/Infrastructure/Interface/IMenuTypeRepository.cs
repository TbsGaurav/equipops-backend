using SettingService.Api.ViewModels.Request.Menu_type;
using SettingService.Api.ViewModels.Response.Menu_type;

namespace SettingService.Api.Infrastructure.Interface
{
    public interface IMenuTypeRepository
    {
        #region Menu Type
        Task<MenuTypeCreateUpdateResponseViewModel> CreateUpdateMenuTypeAsync(MenuTypeCreateUpdateRequestViewModel request);
        Task<MenuTypeListResponseViewModel> GetMenuTypeListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null);
        Task<MenuTypeData> GetMenuTypeByIdAsync(Guid? Id);
        Task DeleteMenuTypeAsync(MenuTypeDeleteRequestViewModel request);
        #endregion

        #region Menu Permission
        Task<MenuPermissionDeleteResponseViewModel> MenuPermissionDeleteAsync(MenuPermissionDeleteRequestViewModel request);
        Task<MenuPermissionCreateUpdateResponseViewModel> CreateUpdateMenuPermissionAsync(MenuPermissionCreateUpdateRequestViewModel request);
        Task<MenuPermissionListResponseViewModel> MenuPermissionListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection);
        Task<MenuPermissionResponseViewModel> MenuPermissionByIdAsync(Guid? id);
        #endregion
    }
}
