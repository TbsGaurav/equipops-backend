namespace SettingService.Api.ViewModels.Response.UserRole
{
    public class UserRoleListResponseViewModel : CommonParameterList
    {
        public List<UserRoleResponseViewModel> userRoleResponseViewModel { get; set; } = new List<UserRoleResponseViewModel>();
    }
    public class UserRoleResponseViewModel : CommonParameterAllList
    {
        public Guid? id { get; set; }
        public string name { get; set; } = null!;
    }

    public class UserRoleDeleteResponseViewModel
    {
        public Guid? id { get; set; }
    }
}