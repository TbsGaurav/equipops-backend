namespace SettingService.Api.ViewModels.Request.Language
{
    public class LanguageCreateUpdateRequestViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Direction { get; set; } = "ltr";
    }
}
