namespace SettingService.Api.ViewModels.Response.EmailTemplate
{
    public class EmailTemplateCreateUpdateResponseViewModel
    {
        public Guid? id { get; set; }
        public string type { get; set; } = string.Empty;
        public string subject { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
    }
    public class EmailTemplateListResponseViewModel : CommonParameterList
    {
        public List<EmailTemplateResponseViewModel> EmailTemplateData { get; set; } = new List<EmailTemplateResponseViewModel>();
    }
    public class EmailTemplateResponseViewModel : CommonParameterAllList
    {
        public Guid? id { get; set; }
        public string type { get; set; } = string.Empty;
        public string subject { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
    }

    public class EmailTemplateDeleteResponseViewModel
    {
        public Guid? id { get; set; }
    }
}
