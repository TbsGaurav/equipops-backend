using Microsoft.AspNetCore.Mvc;

using SettingService.Api.Services.Interface;
using SettingService.Api.ViewModels.Request.Language;

namespace SettingService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageController(ILogger<LanguageController> logger, ILanguageService languageService) : ControllerBase
    {

        [HttpPost("createUpdate")]
        public async Task<IActionResult> CreateOrUpdateLanguage(LanguageCreateUpdateRequestViewModel request)
        {
            var result = await languageService.CreateUpdateLanguageAsync(request);
            return result;
        }
        [HttpGet("list")]
        public async Task<IActionResult> GetLanguageList(string? search, int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "Asc", bool? isActive = null)
        {
            var result = await languageService.GetLanguageListAsync(search, length, page, orderColumn, orderDirection, isActive);
            return result;
        }
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteLanguage([FromBody] LanguageDeleteRequestViewModel request)
        {
            //call service to delete language
            var result = await languageService.DeleteLanguageAsync(request);
            return result;
        }
        [HttpGet("getById")]
        public async Task<IActionResult> GetLanguageById(Guid? Id)
        {
            var result = await languageService.GetLanguageByIdAsync(Id);
            return result;
        }
    }
}
