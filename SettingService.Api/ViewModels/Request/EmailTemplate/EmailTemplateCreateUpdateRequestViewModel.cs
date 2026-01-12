namespace SettingService.Api.ViewModels.Request.EmailTemplate
{
    public class EmailTemplateCreateUpdateRequestViewModel
    {
        public Guid? id { get; set; }
        public string type { get; set; } = string.Empty;
        public string subject { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
    }

    public class EmailTemplateDeleteRequestViewModel
    {
        public Guid? id { get; set; }
    }
}