using Microsoft.AspNetCore.Mvc;

using SettingService.Api.ViewModels.Request.Language;

namespace SettingService.Api.Services.Interface
{
    public interface ILanguageService
    {
        Task<IActionResult> CreateUpdateLanguageAsync(LanguageCreateUpdateRequestViewModel model);
        Task<IActionResult> GetLanguageListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null);
        Task<IActionResult> GetLanguageByIdAsync(Guid? Id);
        Task<IActionResult> DeleteLanguageAsync(LanguageDeleteRequestViewModel request);
    }
}
