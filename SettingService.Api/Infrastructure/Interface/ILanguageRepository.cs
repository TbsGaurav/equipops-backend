using SettingService.Api.ViewModels.Request.Language;
using SettingService.Api.ViewModels.Response.Language;

namespace SettingService.Api.Infrastructure.Interface
{
    public interface ILanguageRepository
    {
        Task<LanguageCreateUpdateResponseViewModel> CreateUpdateLanguageAsync(LanguageCreateUpdateRequestViewModel request);
        Task<LanguageListResponseViewModel> GetLanguageListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null);
        Task<LanguageData> GetLanguageByIdAsync(Guid? Id);
        Task DeleteLanguageAsync(LanguageDeleteRequestViewModel request);
    }
}
