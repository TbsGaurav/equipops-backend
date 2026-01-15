using EquipOps.Model.EquipmentFailure;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class EquipmentFailureController : ControllerBase
    {
        private readonly IEquipmentFailureService _equipmentFailureService;

        public EquipmentFailureController(IEquipmentFailureService equipmentFailureService)
        {
            _equipmentFailureService = equipmentFailureService;
        }

        [HttpPost("equipmentFailureCreateUpdate")]
        public async Task<IActionResult> EquipmentFailureCreate([FromBody] EquipmentFailureRequest request)
        {
            var result = await _equipmentFailureService.EquipmentFailureCreateAsync(request);
            return Ok(result);
        }

        [HttpGet("equipmentFailureList")]
        public async Task<IActionResult> GetEquipmentFailureList(string? search = "", int length = 10, int page = 1, string orderColumn = "failure_date", string orderDirection = "ASC")
        {
            var result = await _equipmentFailureService.EquipmentFailureListAsync(search, length, page, orderColumn, orderDirection);
            return Ok(result);
        }

        [HttpGet("equipmentFailureById")]
        public async Task<IActionResult> GetEquipmentFailureById(int failure_id)
        {
            var result = await _equipmentFailureService.EquipmentFailureByIdAsync(failure_id);
            return Ok(result);
        }

        [HttpPost("equipmentFailureDelete")]
        public async Task<IActionResult> EquipmentFailureDelete([FromBody] EquipmentFailureDeleteRequestViewModel request)
        {
            var result = await _equipmentFailureService.EquipmentFailureDeleteAsync(request.failure_id);
            return Ok(result);
        }
    }
}
