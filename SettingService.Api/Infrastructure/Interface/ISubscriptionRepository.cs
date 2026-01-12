using Microsoft.AspNetCore.Mvc;
using SettingService.Api.ViewModels.Request.Subscription;
using SettingService.Api.ViewModels.Request.SubscriptionType;
using SettingService.Api.ViewModels.Response.Subscription;
using SettingService.Api.ViewModels.Response.SubscriptionType;

namespace SettingService.Api.Infrastructure.Interface
{
	public interface ISubscriptionRepository
	{
		#region Subscription
		Task<SubscriptionCreateUpdateResponseViewModel> CreateUpdateSubscriptionAsync(SubscriptionCreateUpdateRequestViewModel request);
		Task<SubscriptionListResponseViewModel> GetSubscriptionListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null);
		Task DeleteSubscriptionAsync(SubscriptionDeleteRequestViewModel request);
		Task<SubscriptionData> GetSubscriptionByIdAsync(Guid? Id);
		Task<OrganizationSubscriptionResponseViewModel> GetSubscriptionByOrganizationIdAsync(Guid? OrganizationId);
		#endregion

		#region Subscription Type
		Task<SubscriptionTypeDeleteResponseViewModel> SubscriptionTypeDeleteAsync(SubscriptionTypeDeleteRequestViewModel request);
		Task<SubscriptionTypeCreateUpdateResponseViewModel> CreateUpdateSubscriptionTypeAsync(SubscriptionTypeCreateUpdateRequestViewModel request);
		Task<SubscriptionTypeListResponseViewModel> SubscriptionTypeListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection);

		Task<SubscriptionTypeResponseViewModel> SubscriptionTypeByIdAsync(Guid id);
		#endregion
	}
}

