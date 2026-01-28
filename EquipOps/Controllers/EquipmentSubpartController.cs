using CommonHelper.Enums;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.EquipmentSubpart;
using EquipOps.Services.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class EquipmentSubpartController(ILogger<EquipmentSubpartController> logger, IEquipmentSubpartService _equipmentSubpartService) : ControllerBase
    {
        [HttpPost("equipmentSubpartCreateUpdate")]

        public async Task<IActionResult> EquipmentSubpartCreateUpdate([FromBody] EquipmentSubpartRequest request, [FromServices] IValidator<EquipmentSubpartRequest> validator)
        {
            logger.LogInformation("API hit: CreateOrUpdate. Name={Name}", request.subpart_name);

            var result = await validator.ValidateAsync(request);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ResponseHelper<object>.Error(message: "Validation failed.", errors: errors, statusCode: StatusCodeEnum.BAD_REQUEST));
            }
            return await _equipmentSubpartService.EquipmentSubpartCreateUpdateAsync(request);
        }

        [HttpGet("equipmentSubpartList")]
        public async Task<IActionResult> GetEquipmentSubpartList(string? search = "", bool? status = null, int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "ASC")
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
