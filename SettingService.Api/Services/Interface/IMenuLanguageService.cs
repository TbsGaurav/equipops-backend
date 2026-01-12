using Microsoft.AspNetCore.Mvc;

using SettingService.Api.ViewModels.Request.MenuLanguage;

namespace SettingService.Api.Services.Interface
{
    public interface IMenuLanguageService
    {
        Task<IActionResult> CreateUpdateMenuLanguageAsync(MenuLanguageCreateUpdateRequestViewModel model);
        Task<IActionResult> GetMenuLanguageListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null);
        Task<IActionResult> GetMenuLanguageByIdAsync(Guid? Id);
        Task<IActionResult> DeleteMenuLanguageAsync(MenuLanguageDeleteRequestViewModel request);
        Task<IActionResult> GetMenuLanguageByLanguageAsync(Guid? languageId);
        Task<IActionResult> MenuLanguageByLanguageUpdateAsync(MenuLanguageByLanguageUpdateRequestViewModel request);
    }
}
