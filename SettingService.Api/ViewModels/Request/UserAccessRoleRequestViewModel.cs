namespace SettingService.Api.ViewModels.Request
{
    public class UserAccessRoleRequestViewModel
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

    public class UserAccessRoleDeleteRequestViewModel
    {
        public Guid? id { get; set; }
    }
}
