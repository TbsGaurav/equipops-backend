using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SettingService.Api.Helpers.ResponseHelpers.Enums;
using SettingService.Api.Helpers.ResponseHelpers.Handlers;
using SettingService.Api.Services.Interface;
using SettingService.Api.ViewModels.Request.Subscription;
using SettingService.Api.ViewModels.Request.SubscriptionType;

namespace SettingService.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SubscriptionController(ILogger<SubscriptionController> logger, ISubscriptionService subscriptionService) : ControllerBase
	{
		#region Subscription
		[HttpPost("createUpdate")]
		public async Task<IActionResult> CreateOrUpdateSubscription(SubscriptionCreateUpdateRequestViewModel request)
		{
			var result = await subscriptionService.CreateUpdateSubscriptionAsync(request);
			return result;
		}
		[HttpGet("list")]
		public async Task<IActionResult> GetSubscriptionList(string? search, int length = 10, int page = 1, string orderColumn = "date", string orderDirection = "Asc", bool? isActive = null)
		{
			var result = await subscriptionService.GetSubscriptionListAsync(
				search, length, page, orderColumn, orderDirection, isActive);
			return result;
		}

		[HttpGet("getById")]
		public async Task<IActionResult> GetSubscriptionById(Guid? Id)
		{
			var result = await subscriptionService.GetSubscriptionByIdAsync(Id);
			return result;
		}

		[HttpPost("delete")]
		public async Task<IActionResult> DeleteSubscription([FromBody] SubscriptionDeleteRequestViewModel request)
		{
			var result = await subscriptionService.DeleteSubscriptionAsync(request);
			return result;
		}

		[AllowAnonymous]
		[HttpGet("getByOrganizationId")]
		public async Task<IActionResult> GetSubscriptionByOrganizationId(Guid? OrganizationId)
		{
			var result = await subscriptionService.GetSubscriptionByOrganizationIdAsync(OrganizationId);
			return result;
		}
		#endregion Subscription

		#region Subscription Type
		[HttpPost("createUpdateType")]
		public async Task<IActionResult> CreateOrUpdateSubscriptionType(SubscriptionTypeCreateUpdateRequestViewModel request)
		{
			if (!ModelState.IsValid)
			{
				var errors = ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.ToList();

				logger.LogWarning("Validation failed.");

				return BadRequest(ResponseHelper<string>.Error(
					"Validation failed",
					errors: errors,
					statusCode: StatusCodeEnum.BAD_REQUEST
				));
			}

			var result = await subscriptionService.CreateUpdateSubscriptionTypeAsync(request);

			logger.LogInformation(
				"Service response: Success={Success}",
				result.Success);

			if (!result.Success)
			{
				return Conflict(ResponseHelper<string>.Error(
					result.Message ?? "Subscription type create/update failed.",
					statusCode: StatusCodeEnum.CONFLICT_OCCURS
				));
			}

			return Ok(result);
		}

		[HttpGet("SubscriptionTypeList")]
		public async Task<IActionResult> GetSubscriptionTypeList(string? search = "", bool? IsActive = null, int length = 10, int page = 1, string orderColumn = "type", string orderDirection = "ASC")
		{
			var result = await subscriptionService.SubscriptionTypeListAsync(search, IsActive, length, page, orderColumn, orderDirection);

			return Ok(result);
		}

		[HttpPost("SubscriptionTypeDelete")]
		public async Task<IActionResult> SubscriptionTypeDelete([FromBody] SubscriptionTypeDeleteRequestViewModel request)
		{
			var result = await subscriptionService.SubscriptionTypeDeleteAsync(request);
			return Ok(result);
		}

		[HttpGet("SubscriptionTypeById")]
		public async Task<IActionResult> GetSubscriptionType_ById(Guid id)
		{
			var result = await subscriptionService.SubscriptionTypeByIdAsync(id);

			return Ok(result);
		}

		#endregion
	}
}
