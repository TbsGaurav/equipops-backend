using CommonHelper.Enums;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.EquipmentFailure;
using EquipOps.Services.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class EquipmentFailureController(ILogger<EquipmentFailureController> logger, IEquipmentFailureService _equipmentFailureService) : ControllerBase
    {
        [HttpPost("equipmentFailureCreateUpdate")]
        public async Task<IActionResult> FailureCreateUpdate([FromBody] EquipmentFailureRequest request, [FromServices] IValidator<EquipmentFailureRequest> validator)
        {
            logger.LogInformation("API hit: CreateOrUpdate. Name={Name}", request.failure_type);

            var result = await validator.ValidateAsync(request);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ResponseHelper<object>.Error(message: "Validation failed.", errors: errors, statusCode: StatusCodeEnum.BAD_REQUEST));
            }
            return await _equipmentFailureService.EquipmentFailureCreateUpdateAsync(request);
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
