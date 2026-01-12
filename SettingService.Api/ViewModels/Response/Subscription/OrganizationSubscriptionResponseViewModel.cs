namespace SettingService.Api.ViewModels.Response.Subscription
{
	public class OrganizationSubscriptionResponseViewModel
	{
		public Guid Id { get; set; }
		public Guid Organization_Id { get; set; }
		public Guid Subscription_Type_Id { get; set; }
		public string Type { get; set; } = string.Empty;
		public int Resume_Matching { get; set; }
		public int Interview_Create { get; set; }
		public int Interview_Schedule { get; set; }
		public decimal Price { get; set; }

	}
}
