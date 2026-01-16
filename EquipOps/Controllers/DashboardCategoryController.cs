using CommonHelper.Enums;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.Dashboard;
using EquipOps.Services.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
	[AllowAnonymous]
	[ApiController]
	[Route("api/[controller]")]
	public class DashboardCategoryController(ILogger<DashboardCategoryController> logger, IDashboardCategoryService dashboardCategoryService) : ControllerBase
	{
		[HttpPost("create-update")]
		public async Task<IActionResult> CreateOrUpdateDashboardCategory([FromBody] DashboardCategoryRequest request,[FromServices] IValidator<DashboardCategoryRequest> validator)
		{
			logger.LogInformation("API hit: CreateOrUpdateDashboardCategory. Name={Name}",request.Name);
			var result = await validator.ValidateAsync(request);
			if (!result.IsValid)
			{
				var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
				return BadRequest(ResponseHelper<object>.Error(message: "Validation failed.",errors: errors,statusCode: StatusCodeEnum.BAD_REQUEST));
			}
			return await dashboardCategoryService.AddOrUpdateAsync(request);
		}

		[HttpGet("get-by-id")]
		public async Task<IActionResult> GetDashboardCategoryById([FromQuery] int id)
		{
			logger.LogInformation("API hit: GetDashboardCategoryById. Id={Id}",id);
			return await dashboardCategoryService.GetByIdAsync(id);
		}

		[HttpGet("list")]
		public async Task<IActionResult> GetDashboardCategoryList([FromQuery] string? search = null,[FromQuery] int length = 10,[FromQuery] int page = 1,[FromQuery] string orderColumn = "name",[FromQuery] string orderDirection = "Asc")
		{
			logger.LogInformation("API hit: GetDashboardCategoryList | Search={Search}, Page={Page}, Length={Length}",search, page, length);
			return await dashboardCategoryService.GetListAsync(search,length,page,orderColumn,orderDirection);
		}

		[HttpPost("delete")]
		public async Task<IActionResult> DeleteDashboardCategory([FromQuery] int id)
		{
			logger.LogInformation("API hit: DeleteDashboardCategory. Id={Id}",id);
			return await dashboardCategoryService.DeleteAsync(id);
		}
	}

}
