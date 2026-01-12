using NpgsqlTypes;

namespace OrganizationService.Api.ViewModels.Request.User
{
    public class UserCreateUpdateRequestViewModel
    {
        public Guid? Id { get; set; }
        public string First_Name { get; set; } = string.Empty;
        public string Last_Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone_Number { get; set; } = string.Empty;
        public Guid? Organization_Id { get; set; }
        public string Role_Name { get; set; } = string.Empty;
        public Guid? Language_Id { get; set; }
        public List<UserAccessRoleBulkDto> UserAccessRole { get; set; }
    }

    public class UserAccessRoleBulkDto
    {
        [PgName("_id")]
        public Guid? id { get; set; }
        [PgName("_menu_permission_id")]
        public Guid menu_permission_id { get; set; }
        [PgName("_is_checked")]
        public bool is_checked { get; set; }
    }
}
