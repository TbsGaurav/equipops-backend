namespace SettingService.Api.ViewModels.Response.Menu_type
{
    public class MenuTypeCreateUpdateResponseViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Is_delete { get; set; }
        public bool Is_active { get; set; }
        public Guid? Created_by { get; set; }
        public DateTime? Created_date { get; set; }
        public Guid? Updated_by { get; set; }
        public DateTime? Updated_date { get; set; }
    }
}
