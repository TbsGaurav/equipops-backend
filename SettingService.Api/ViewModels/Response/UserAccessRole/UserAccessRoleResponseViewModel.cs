namespace SettingService.Api.ViewModels.Response.UserAccessRole
{
    public class UserAccessRoleCreateUpdateResponseViewModel
    {
        public Guid? id { get; set; }
        public Guid organization_id { get; set; }
        public Guid user_id { get; set; }
        public string? menu_name { get; set; }
        public bool create { get; set; }
        public bool update { get; set; }
        public bool delete { get; set; }
        public bool view { get; set; }
    }

    public class UserAccessRoleListResponseViewModel : CommonParameterList
    {
        public List<UserAccessRoleResponseViewModel> userRoleResponseViewModel { get; set; } = new List<UserAccessRoleResponseViewModel>();
    }

    public class UserAccessRoleResponseViewModel : CommonParameterAllList
    {
        public Guid? id { get; set; }
        public Guid organization_id { get; set; }
        public Guid user_id { get; set; }
        public string? menu_name { get; set; }
        public bool create { get; set; }
        public bool update { get; set; }
        public bool delete { get; set; }
        public bool view { get; set; }
    }

    public class UserAccessRoleDeleteResponseViewModel
    {
        public Guid? id { get; set; }
    }
}