namespace SettingService.Api.ViewModels.Request
{
    public class UserRoleRequestViewModel
    {
        public Guid? id { get; set; }
        public string name { get; set; } = null!;
    }

    public class UserRoleDeleteRequestViewModel
    {
        public Guid? id { get; set; }
    }
}
