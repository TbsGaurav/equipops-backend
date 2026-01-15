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
    public class EquipmentController : ControllerBase
    {
        private readonly ILogger<EquipmentController> _logger;
        private readonly IEquipmentService _equipmentService;

		public EquipmentController(ILogger<EquipmentController> logger,IEquipmentService equipmentService)
		{
			_logger = logger;
			_equipmentService = equipmentService;
		}

		[HttpPost("create-update")]
		public async Task<IActionResult> CreateOrUpdateEquipment(
	[FromBody] EquipmentRequest request,
	[FromServices] IValidator<EquipmentRequest> validator)
		{
			_logger.LogInformation("API hit: CreateOrUpdateEquipment. Name={Name}", request.Name);

			var result = await validator.ValidateAsync(request);
			if (!result.IsValid)
			{
				var errors = result.Errors
					.Select(e => e.ErrorMessage)
					.ToList();

				return BadRequest(
					ResponseHelper<object>.Error(
						message: "Validation failed.",
						errors: errors,
						statusCode: StatusCodeEnum.BAD_REQUEST
					)
				);
			}

			return await _equipmentService.AddOrUpdateAsync(request);
		}


		[HttpGet("get-by-id")]
		public async Task<IActionResult> GetEquipmentById([FromQuery] int id)
		{
			_logger.LogInformation("API hit: GetEquipmentById. EquipmentId={EquipmentId}", id);

			return await _equipmentService.GetByIdAsync(id);
		}

		[HttpGet("list")]
		public async Task<IActionResult> GetEquipmentList(
			[FromQuery] string? search = null,
			[FromQuery] int length = 10,
			[FromQuery] int page = 1,
			[FromQuery] string orderColumn = "name",
			[FromQuery] string orderDirection = "Asc")
		{
			_logger.LogInformation(
				"API hit: GetEquipmentList | Search={Search}, Page={Page}, Length={Length}",
				search, page, length);

			return await _equipmentService.GetEquipmentsAsync(
				search,
				length,
				page,
				orderColumn,
				orderDirection
			);
		}

		[HttpPost("delete")]
		public async Task<IActionResult> DeleteEquipment([FromQuery] int equipmentId)
		{
			_logger.LogInformation(
				"API hit: DeleteEquipment. EquipmentId={EquipmentId}",
				equipmentId);

			return await _equipmentService.DeleteAsync(equipmentId);
		}
	}

}
