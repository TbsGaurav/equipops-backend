using Microsoft.AspNetCore.Mvc;

using SettingService.Api.Services.Interface;
using SettingService.Api.ViewModels.Request.MenuLanguage;

namespace SettingService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuLanguageController(IMenuLanguageService menuLanguageService) : ControllerBase
    {
        [HttpPost("createUpdate")]
        public async Task<IActionResult> CreateOrUpdateMenuLanguage(MenuLanguageCreateUpdateRequestViewModel request)
        {
            var result = await menuLanguageService.CreateUpdateMenuLanguageAsync(request);
            return result;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetMenuLanguageList(string? search, int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "Asc", bool? isActive = null)
        {
            var result = await menuLanguageService.GetMenuLanguageListAsync(search, length, page, orderColumn, orderDirection, isActive);
            return result;
        }

        [HttpGet("getById")]
        public async Task<IActionResult> GetMenuLanguageById(Guid? Id)
        {
            var result = await menuLanguageService.GetMenuLanguageByIdAsync(Id);
            return result;
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteMenuLanguage([FromBody] MenuLanguageDeleteRequestViewModel request)
        {
            var result = await menuLanguageService.DeleteMenuLanguageAsync(request);
            return result;
        }

        [HttpGet("getByLanguage")]
        public async Task<IActionResult> GetMenuLanguageByLanguage(Guid? languageId)
        {
            var result = await menuLanguageService.GetMenuLanguageByLanguageAsync(languageId);
            return result;
        }

        [HttpPost("updateByLanguage")]
        public async Task<IActionResult> UpdateMenuLanguageByLanguage([FromBody] MenuLanguageByLanguageUpdateRequestViewModel request)
        {
            var result = await menuLanguageService.MenuLanguageByLanguageUpdateAsync(request);
            return result;
        }
    }
}
