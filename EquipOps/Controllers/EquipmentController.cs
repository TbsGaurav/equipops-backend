using CommonHelper.Enums;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.Requests.Equipment;
using EquipOps.Services.Implementation;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.API.Controller
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController(ILogger<EquipmentController> logger, IEquipmentService equipmentService) : ControllerBase
    {
		[HttpPost("create-update")]
		public async Task<IActionResult> CreateOrUpdateEquipment([FromBody] EquipmentRequest request,[FromServices] IValidator<EquipmentRequest> validator)
		{
			logger.LogInformation("API hit: CreateOrUpdateEquipment. Name={Name}", request.Name);

			var result = await validator.ValidateAsync(request);
			if (!result.IsValid)
			{
				var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
				return BadRequest(ResponseHelper<object>.Error(message: "Validation failed.",errors: errors,statusCode: StatusCodeEnum.BAD_REQUEST));
			}
			return await equipmentService.AddOrUpdateAsync(request);
		}

		[HttpGet("get-by-id")]
		public async Task<IActionResult> GetEquipmentById([FromQuery] int id)
		{
			logger.LogInformation("API hit: GetEquipmentById. EquipmentId={EquipmentId}", id);
			return await equipmentService.GetByIdAsync(id);
		}
		[HttpGet("list")]
		public async Task<IActionResult> GetEquipmentList([FromQuery] string? search = null,[FromQuery] int length = 10,[FromQuery] int page = 1,[FromQuery] string orderColumn = "name",[FromQuery] string orderDirection = "Asc",[FromQuery] int? isActive = -1)
		{
			logger.LogInformation("API hit: GetEquipmentList | Search={Search}, Page={Page}, Length={Length}, IsActive={IsActive}",search, page, length, isActive);
			return await equipmentService.GetEquipmentsAsync(search,length,page,orderColumn,orderDirection,isActive);
		}

		[HttpPost("delete")]
		public async Task<IActionResult> DeleteEquipment([FromQuery] int equipmentId)
		{
			logger.LogInformation("API hit: DeleteEquipment. EquipmentId={EquipmentId}",equipmentId);
			return await equipmentService.DeleteAsync(equipmentId);
		}

        [HttpGet("Dropdown")]
        public async Task<IActionResult> EquipmentDropdown()
        {
            return await equipmentService.EquipmentDropdownAsync();
        }
    }
}
