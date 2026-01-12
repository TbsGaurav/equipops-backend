using SettingService.Api.ViewModels.Request.MenuLanguage;
using SettingService.Api.ViewModels.Response.MenuLanguage;

namespace SettingService.Api.Infrastructure.Interface
{
    public interface IMenuLanguageRepository
    {
        Task<MenuLanguageCreateUpdateResponseViewModel> CreateUpdateMenuLanguageAsync(MenuLanguageCreateUpdateRequestViewModel request);
        Task<MenuLanguageListResponseViewModel> GetMenuLanguageListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null);
        Task<MenuLanguageData> GetMenuLanguageByIdAsync(Guid? Id);
        Task DeleteMenuLanguageAsync(MenuLanguageDeleteRequestViewModel request);
        Task<Dictionary<string, string>> GetMenuLanguageByLanguageAsync(Guid? languageId);
        Task MenuLanguageByLanguageUpdateAsync(MenuLanguageByLanguageUpdateRequestViewModel request);
    }
}
