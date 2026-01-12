namespace SettingService.Api.ViewModels.Request.SubscriptionType
{
    public class SubscriptionTypeCreateUpdateRequestViewModel
    {
        public Guid? id { get; set; }
        public string type { get; set; } = string.Empty;
        public int resume_matching { get; set; }
        public int interview_create { get; set; }
        public int interview_schedule { get; set; }
        public decimal price { get; set; }
        public int duration { get; set; }
        public string description { get; set; } = string.Empty;
    }


    public class SubscriptionTypeDeleteRequestViewModel
    {
        public Guid? id { get; set; }
    }
}
