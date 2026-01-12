namespace SettingService.Api.ViewModels.Response.Master_Dropdown
{
    public class MasterDropdownListResponseViewModel
    {
        public List<MasterDropdownItemViewModel> Roles { get; set; } = [];
        public List<MasterDropdownItemViewModel> Organizations { get; set; } = [];
        public List<LanguageDropdownItemViewModel> Languages { get; set; } = [];
        public List<MasterDropdownItemViewModel> Menu_Types { get; set; } = [];
        public List<IndustryTypeDropdownItemViewModel> Industry_Types { get; set; } = [];
        public List<MenuPermissionDropdownItemViewModel> Menu_Permissions { get; set; } = [];
    }
}
