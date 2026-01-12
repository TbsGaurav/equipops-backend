namespace SettingService.Api.ViewModels.Response.Menu_type
{
    public class MenuPermissionCreateUpdateResponseViewModel
    {
        public Guid? id { get; set; }
        public Guid menu_type_id { get; set; }
        public string menu_type_name { get; set; } = string.Empty;
        public string slug { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
    }
    public class MenuPermissionListResponseViewModel : CommonParameterList
    {
        public List<MenuPermissionResponseViewModel> MenuPermissionData { get; set; } = new List<MenuPermissionResponseViewModel>();
    }
    public class MenuPermissionResponseViewModel : CommonParameterAllList
    {
        public Guid? id { get; set; }
        public Guid menu_type_id { get; set; }
        public string menu_type_name { get; set; } = string.Empty;
        public string slug { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
    }

    public class MenuPermissionDeleteResponseViewModel
    {
        public Guid? id { get; set; }
    }
}


