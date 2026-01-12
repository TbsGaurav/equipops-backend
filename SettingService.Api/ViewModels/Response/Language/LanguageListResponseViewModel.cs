namespace SettingService.Api.ViewModels.Response.Language
{
    public class LanguageListResponseViewModel
    {
        public int TotalNumbers { get; set; }
        public List<LanguageData> LanguageData { get; set; } = new List<LanguageData>();
    }
    public class LanguageData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Direction { get; set; } = "ltr";
        public bool Is_active { get; set; }
        public bool Is_delete { get; set; }
        public Guid? Created_by { get; set; }
        public DateTime? Created_date { get; set; }
        public Guid? Updated_by { get; set; }
        public DateTime? Updated_date { get; set; }
    }
}
