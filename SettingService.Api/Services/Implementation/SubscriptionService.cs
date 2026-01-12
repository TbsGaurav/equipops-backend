using Microsoft.AspNetCore.Mvc;

using SettingService.Api.Helpers.ResponseHelpers.Enums;
using SettingService.Api.Helpers.ResponseHelpers.Handlers;
using SettingService.Api.Helpers.ResponseHelpers.Models;
using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.Services.Interface;
using SettingService.Api.ViewModels.Request.Subscription;
using SettingService.Api.ViewModels.Request.SubscriptionType;
using SettingService.Api.ViewModels.Response.Subscription;
using SettingService.Api.ViewModels.Response.SubscriptionType;

namespace SettingService.Api.Services.Implementation
{
	public class SubscriptionService(ILogger<SubscriptionService> _logger, ISubscriptionRepository subscriptionRepository) : ISubscriptionService
	{
		#region Subscription
		public async Task<IActionResult> CreateUpdateSubscriptionAsync(SubscriptionCreateUpdateRequestViewModel model)
		{
			_logger.LogInformation("SubscriptionService: CreateUpdate START. Organization_id={Organization_id}", model.Organization_id);
			if (model.Organization_id == Guid.Empty ||
				model.Subscription_type_id == Guid.Empty ||
				model.Date == default)
			{
				_logger.LogWarning("Validation failed for required fields.");
				return new BadRequestObjectResult
				(ResponseHelper<string>.Error(
					 "Organization, Subscription Type and Date are required.",
					 statusCode: StatusCodeEnum.BAD_REQUEST
				)
				);
			}
			var data = await subscriptionRepository.CreateUpdateSubscriptionAsync(model);
			if (data == null || data.Id == Guid.Empty)
			{
				return new ObjectResult(
				  ResponseHelper<string>.Error(
					  "Subscription create/update failed.",
					  statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
				  )
			  );
			}
			return new OkObjectResult(
				ResponseHelper<SubscriptionCreateUpdateResponseViewModel>.Success(
					model.Id == Guid.Empty
						? "Subscription created successfully."
						: "Subscription updated successfully.",
					data
				)
			);
		}

		public async Task<IActionResult> GetSubscriptionListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null)
		{
			_logger.LogInformation(
				"SubscriptionService: Fetching subscription list. Search={Search}, Page={Page}, Length={Length}",
				Search, Page, Length);

			var data = await subscriptionRepository.GetSubscriptionListAsync(Search, Length, Page, OrderColumn, OrderDirection, isActive);
			return new OkObjectResult(
				ResponseHelper<SubscriptionListResponseViewModel>.Success(
				   "Subscription list fetched successfully.",
					data
				)
			);
		}

		public async Task<IActionResult> DeleteSubscriptionAsync(SubscriptionDeleteRequestViewModel request)
		{
			_logger.LogInformation("SubscriptionService: Deleting subscription with SubscriptionId={SubscriptionId}", request.Id);

			// Call repository to delete the subscription
			await subscriptionRepository.DeleteSubscriptionAsync(request);

			// fetch updated list after deletion
			var updatedList = await subscriptionRepository.GetSubscriptionListAsync(
				Search: null,
				Length: 10,
				Page: 1,
				OrderColumn: "date",
				OrderDirection: "Asc"
			);
			return new OkObjectResult(
			  ResponseHelper<SubscriptionListResponseViewModel>.Success(
				 "Subscription list fetched successfully.",
				  updatedList
			  )
		  );
		}

		public async Task<IActionResult> GetSubscriptionByIdAsync(Guid? Id)
		{
			_logger.LogInformation(
				"SubscriptionService: Fetching subscription. Id={Id}", Id);

			var data = await subscriptionRepository.GetSubscriptionByIdAsync(Id);
			return new OkObjectResult(
			ResponseHelper<SubscriptionData>.Success(
			   "Subscription fetched successfully.",
				data
			)
			);
		}

		public async Task<IActionResult> GetSubscriptionByOrganizationIdAsync(Guid? OrganizationId)
		{
			_logger.LogInformation("SubscriptionService: Fetching subscription. OrganizationId={OrganizationId}", OrganizationId);

			var data = await subscriptionRepository.GetSubscriptionByOrganizationIdAsync(OrganizationId);

			return new OkObjectResult(
				ResponseHelper<OrganizationSubscriptionResponseViewModel>.Success(
				   "Subscription fetched successfully.",
					data
				)
			);
		}
		#endregion

		#region Subscription type
		public async Task<ApiResponse<SubscriptionTypeDeleteResponseViewModel>> SubscriptionTypeDeleteAsync(SubscriptionTypeDeleteRequestViewModel model)
		{
			//🔹 Repository Call

			var data = await subscriptionRepository.SubscriptionTypeDeleteAsync(model);

			string Message = "";
			bool Status = false;

			if (data.id == null)
				Message = "Invalid data";
			else
			{
				Status = true;
				Message = "User role is deleted successfully.";
			}

			return new ApiResponse<SubscriptionTypeDeleteResponseViewModel>
			{
				StatusCode = 1,
				Success = Status,
				Message = Message,
				Data = data
			};
		}
		public async Task<ApiResponse<SubscriptionTypeCreateUpdateResponseViewModel>> CreateUpdateSubscriptionTypeAsync(SubscriptionTypeCreateUpdateRequestViewModel model)
		{
			var data = await subscriptionRepository.CreateUpdateSubscriptionTypeAsync(model);
			if (data == null || data.id == Guid.Empty)
			{
				return new ApiResponse<SubscriptionTypeCreateUpdateResponseViewModel>
				{
					Success = false,
					Message = "Subscription type create/update failed.",
					Data = data
				};
			}
			return new ApiResponse<SubscriptionTypeCreateUpdateResponseViewModel>
			{
				StatusCode = 200,
				Success = true,
				Message = model.id == null
					? "Subscription type created successfully."
					: "Subscription type updated successfully.",
				Data = data
			};
		}

		public async Task<ApiResponse<SubscriptionTypeListResponseViewModel>> SubscriptionTypeListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection)
		{
			string Message = "";
			bool Status = false;

			//🔹 Repository Call
			var data = await subscriptionRepository.SubscriptionTypeListAsync(search, IsActive, length, page, orderColumn, orderDirection);

			if (data == null)
				Message = "Invalid data.";
			else
			{
				Status = true;
				Message = "Success.";
			}

			return new ApiResponse<SubscriptionTypeListResponseViewModel>
			{
				StatusCode = 200,
				Success = Status,
				Message = Message,
				Data = data
			};
		}
		public async Task<ApiResponse<SubscriptionTypeResponseViewModel>> SubscriptionTypeByIdAsync(Guid id)
		{
			_logger.LogInformation("SubscriptionService: Fetching subscription list. Search={id}", id);

			var data = await subscriptionRepository.SubscriptionTypeByIdAsync(id);

			return new ApiResponse<SubscriptionTypeResponseViewModel>
			{
				Success = true,
				Message = "Subscription ById fetched successfully.",
				Data = data
			};
		}
		#endregion
	}
}
