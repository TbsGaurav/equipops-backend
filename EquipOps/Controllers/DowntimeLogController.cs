using EquipOps.Model.DowntimeLog;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class DowntimeLogController(IDowntimeLogService downtimeLogService) : ControllerBase
    {
        [HttpPost("downtimeCreateUpdate")]
        public async Task<IActionResult> DowntimeCreateUpdate([FromBody] DowntimeLogRequest request)
        {
            var result = await downtimeLogService.DowntimeLogCreateUpdateAsync(request);
            return Ok(result);
        }
    }
}
