using EquipOps.Model.EquipmentSubpart;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class EquipmentSubpartController : ControllerBase
    {
        private readonly IEquipmentSubpartService _equipmentSubpartService;

        public EquipmentSubpartController(IEquipmentSubpartService equipmentSubpartService)
        {
            _equipmentSubpartService = equipmentSubpartService;
        }

        [HttpPost("equipmentSubpartCreateUpdate")]
        public async Task<IActionResult> EquipmentSubpartCreateUpdate([FromBody] EquipmentSubpartRequest request)
        {
            var result = await _equipmentSubpartService.EquipmentSubpartCreateUpdateAsync(request);
            return Ok(result);
        }

        [HttpGet("equipmentSubpartList")]
        public async Task<IActionResult> GetEquipmentSubpartList(string? search = "", bool? status = null, int length = 10,int page = 1,string orderColumn = "name",string orderDirection = "ASC")
        {
            var result = await _equipmentSubpartService.EquipmentSubpartListAsync(search, status, length, page, orderColumn, orderDirection);
            return Ok(result);
        }

        [HttpGet("equipmentSubpartById")]
        public async Task<IActionResult> GetEquipmentSubpartById(int subpart_id)
        {
            var result = await _equipmentSubpartService.EquipmentSubpartByIdAsync(subpart_id);
            return Ok(result);
        }

        [HttpPost("equipmentSubpartDelete")]
        public async Task<IActionResult> EquipmentSubpartDelete([FromBody] EquipmentSubpartDeleteRequestViewModel request)
        {
            var result = await _equipmentSubpartService.EquipmentSubpartDeleteAsync(request.subpart_id);
            return Ok(result);
        }

        [HttpGet("equipmentSubpart/Dropdown")]
        public async Task<IActionResult> EquipmentSubpartDropdown()
        {
            return await _equipmentSubpartService.EquipmentSubpartDropdownAsync();
        }
    }
}
