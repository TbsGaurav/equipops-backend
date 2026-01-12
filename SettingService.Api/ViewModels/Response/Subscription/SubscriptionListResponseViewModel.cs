namespace SettingService.Api.ViewModels.Response.Subscription
{
    public class SubscriptionListResponseViewModel
    {
        public int TotalNumbers { get; set; }
        public List<SubscriptionData> SubscriptionData { get; set; } = new List<SubscriptionData>();
    }
    public class SubscriptionData
    {
        public Guid Id { get; set; }
        public string Organization { get; set; } = string.Empty;
        public string Subscription_type { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int Resume_maching_pending { get; set; }
        public int Interview_create_pending { get; set; }
        public int Interview_schedule_pending { get; set; }
        public bool Is_delete { get; set; }
        public bool Is_active { get; set; }
        public Guid? Created_by { get; set; }
        public DateTime? Created_date { get; set; }
        public Guid? Updated_by { get; set; }
        public DateTime? Updated_date { get; set; }
    }
}
