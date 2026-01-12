namespace SettingService.Api.ViewModels.Response.MenuLanguage
{
    public class MenuLanguageCreateUpdateResponseViewModel
    {
        public Guid? Id { get; set; }
        public Guid? Language_id { get; set; }
        public string Key_name { get; set; } = string.Empty;
        public string Translate_text { get; set; } = string.Empty;
        public bool Is_delete { get; set; }
        public bool Is_active { get; set; }
        public Guid? Created_by { get; set; }
        public DateTime? Created_date { get; set; }
        public Guid? Updated_by { get; set; }
        public DateTime? Updated_date { get; set; }
    }
}
