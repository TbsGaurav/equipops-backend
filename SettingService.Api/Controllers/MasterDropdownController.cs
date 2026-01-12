using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SettingService.Api.Services.Interface;

namespace SettingService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDropdownController : ControllerBase
    {
        private readonly IMasterDropdownService _masterDropdownService;

        public MasterDropdownController(IMasterDropdownService masterDropdownService)
        {
            _masterDropdownService = masterDropdownService;
        }

        [AllowAnonymous]
        [HttpGet("list")]
        public async Task<IActionResult> GetMasterDropdowns()
        {
            // 🔹 Call Service
            var result = await _masterDropdownService.GetMasterDropdownsAsync();
            return result;
        }
    }
}
