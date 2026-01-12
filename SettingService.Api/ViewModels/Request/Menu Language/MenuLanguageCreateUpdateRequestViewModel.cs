namespace SettingService.Api.ViewModels.Request.MenuLanguage
{
    public class MenuLanguageCreateUpdateRequestViewModel
    {
        public Guid? Id { get; set; }
        public Guid? Language_id { get; set; }
        public string Key_name { get; set; } = string.Empty;
        public string Translate_text { get; set; } = string.Empty;
    }
}
