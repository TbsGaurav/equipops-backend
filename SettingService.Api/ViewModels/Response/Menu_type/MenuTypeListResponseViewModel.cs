namespace SettingService.Api.ViewModels.Response.Menu_type
{
    public class MenuTypeListResponseViewModel
    {
        public int TotalNumbers { get; set; }
        public List<MenuTypeData> MenuTypeData { get; set; } = new List<MenuTypeData>();
    }
    public class MenuTypeData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Is_active { get; set; }
        public bool Is_delete { get; set; }
        public Guid? Created_by { get; set; }
        public DateTime? Created_date { get; set; }
        public Guid? Updated_by { get; set; }
        public DateTime? Updated_date { get; set; }
    }
}
