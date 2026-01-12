namespace SettingService.Api.ViewModels.Response.Master_Dropdown
{
    public class MasterDropdownItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class LanguageDropdownItemViewModel : MasterDropdownItemViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
    }
    public class MenuPermissionDropdownItemViewModel : MasterDropdownItemViewModel
    {
        public string Slug { get; set; } = string.Empty;
    }
    public class IndustryTypeDropdownItemViewModel : MasterDropdownItemViewModel
    {
        public string Type { get; set; } = string.Empty;
    }
}
