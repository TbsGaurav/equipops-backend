namespace SettingService.Api.ViewModels.Request.Menu_type
{
    public class MenuPermissionCreateUpdateRequestViewModel
    {
        public Guid? id { get; set; }
        public Guid menu_type_id { get; set; }
        public string slug { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
    }

    public class MenuPermissionDeleteRequestViewModel
    {
        public Guid? id { get; set; }
    }
}
