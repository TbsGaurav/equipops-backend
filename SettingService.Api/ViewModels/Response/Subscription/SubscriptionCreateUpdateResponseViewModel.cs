namespace SettingService.Api.ViewModels.Response.Subscription
{
    public class SubscriptionCreateUpdateResponseViewModel
    {
        public Guid Id { get; set; }
        public Guid Organization_id { get; set; }
        public Guid Subscription_type_id { get; set; }
        public DateTime Date { get; set; }
        public int Resume_maching_pending { get; set; }
        public int Interview_create_pending { get; set; }
        public int Interview_schedule_pending { get; set; }
    }
}
