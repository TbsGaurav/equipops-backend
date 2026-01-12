using Microsoft.AspNetCore.Mvc;
using SettingService.Api.Helpers.ResponseHelpers.Handlers;
using SettingService.Api.Helpers.ResponseHelpers.Models;
using SettingService.Api.ViewModels.Request.Subscription;
using SettingService.Api.ViewModels.Request.SubscriptionType;
using SettingService.Api.ViewModels.Response.SubscriptionType;

namespace SettingService.Api.Services.Interface
{
	public interface ISubscriptionService
	{
		#region Subscription
		Task<IActionResult> CreateUpdateSubscriptionAsync(SubscriptionCreateUpdateRequestViewModel model);
		Task<IActionResult> GetSubscriptionListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null);
		Task<IActionResult> DeleteSubscriptionAsync(SubscriptionDeleteRequestViewModel request);
		Task<IActionResult> GetSubscriptionByIdAsync(Guid? Id);
		Task<IActionResult> GetSubscriptionByOrganizationIdAsync(Guid? OrganizationId);
		#endregion

		#region Subscription Type
		Task<ApiResponse<SubscriptionTypeDeleteResponseViewModel>> SubscriptionTypeDeleteAsync(SubscriptionTypeDeleteRequestViewModel model);
		Task<ApiResponse<SubscriptionTypeCreateUpdateResponseViewModel>> CreateUpdateSubscriptionTypeAsync(SubscriptionTypeCreateUpdateRequestViewModel model);
		Task<ApiResponse<SubscriptionTypeListResponseViewModel>> SubscriptionTypeListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection);

		Task<ApiResponse<SubscriptionTypeResponseViewModel>> SubscriptionTypeByIdAsync(Guid id);
		#endregion
	}
}
