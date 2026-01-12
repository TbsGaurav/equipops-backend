namespace SettingService.Api.ViewModels.Response.SubscriptionType
{
    public class SubscriptionTypeCreateUpdateResponseViewModel
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
    public class SubscriptionTypeListResponseViewModel : CommonParameterList
    {
        public List<SubscriptionTypeResponseViewModel> SubscriptionTypeData { get; set; } = new List<SubscriptionTypeResponseViewModel>();
    }
    public class SubscriptionTypeResponseViewModel : CommonParameterAllList
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

    public class SubscriptionTypeDeleteResponseViewModel
    {
        public Guid? id { get; set; }
    }
}
