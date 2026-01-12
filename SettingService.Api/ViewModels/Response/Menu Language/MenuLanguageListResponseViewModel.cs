namespace SettingService.Api.ViewModels.Response.MenuLanguage
{
    public class MenuLanguageListResponseViewModel
    {
        public int TotalNumbers { get; set; }
        public List<MenuLanguageData> MenuLanguageData { get; set; } = new List<MenuLanguageData>();
    }
    public class MenuLanguageData
    {
        public Guid Id { get; set; }
        public Guid Language_id { get; set; }
        public string Key_name { get; set; } = string.Empty;
        public string Translate_text { get; set; } = string.Empty;
        public bool Is_active { get; set; }
        public bool Is_delete { get; set; }
        public Guid? Created_by { get; set; }
        public DateTime? Created_date { get; set; }
        public Guid? Updated_by { get; set; }
        public DateTime? Updated_date { get; set; }
    }
}
