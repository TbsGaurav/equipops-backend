namespace SettingService.Api.ViewModels.Response.Language
{
    public class LanguageCreateUpdateResponseViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Direction { get; set; } = "ltr";
    }
}
