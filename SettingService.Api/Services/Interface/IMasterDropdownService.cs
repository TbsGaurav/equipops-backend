using Microsoft.AspNetCore.Mvc;

namespace SettingService.Api.Services.Interface
{
    public interface IMasterDropdownService
    {
        Task<IActionResult> GetMasterDropdownsAsync();
    }
}
