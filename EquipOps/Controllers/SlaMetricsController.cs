using CommonHelper.Enums;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.SlaMetrics;
using EquipOps.Services.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
	[AllowAnonymous]
	[ApiController]
	[Route("api/[controller]")]
	public class SlaMetricsController(
		ILogger<SlaMetricsController> logger,
		ISlaMetricsService slaService) : ControllerBase
	{
		[HttpPost("create-update")]
		public async Task<IActionResult> CreateOrUpdate(
			[FromBody] SlaMetricsRequest request,
			[FromServices] IValidator<SlaMetricsRequest> validator)
		{
			logger.LogInformation("API hit: SLA Create/Update");

			var result = await validator.ValidateAsync(request);
			if (!result.IsValid)
			{
				return BadRequest(
					ResponseHelper<object>.Error(
						"Validation failed.",
						errors: result.Errors.Select(e => e.ErrorMessage).ToList(),
						statusCode: StatusCodeEnum.BAD_REQUEST
					)
				);
			}

			return await slaService.AddOrUpdateAsync(request);
		}

		[HttpGet("get-by-id")]
		public async Task<IActionResult> GetById([FromQuery] int id)
		{
			return await slaService.GetByIdAsync(id);
		}

		[HttpGet("list")]
		public async Task<IActionResult> GetList(
			[FromQuery] int organizationId,
			[FromQuery] DateTime startDate,
			[FromQuery] DateTime endDate,
			[FromQuery] bool? slaBreached,
			[FromQuery] int page = 1,
			[FromQuery] int length = 10)
		{
			return await slaService.GetPagedAsync(organizationId, startDate, endDate, slaBreached, page, length);
		}

		[HttpGet("dashboard-summary")]
		public async Task<IActionResult> DashboardSummary(
			[FromQuery] int organizationId,
			[FromQuery] DateTime startDate,
			[FromQuery] DateTime endDate)
		{
			return await slaService.GetDashboardSummaryAsync(organizationId, startDate, endDate);
		}

		[HttpPost("delete")]
		public async Task<IActionResult> Delete([FromQuery] int slaId)
		{
			return await slaService.DeleteAsync(slaId);
		}
	}
}
