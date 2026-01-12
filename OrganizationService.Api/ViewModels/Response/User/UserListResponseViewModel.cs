namespace OrganizationService.Api.ViewModels.Response.User
{
    public class UserListResponseViewModel
    {
        public int TotalNumbers { get; set; }
        public List<UserData> UserData { get; set; } = new List<UserData>();
    }
    public class UserData
    {
        public Guid Id { get; set; }
        public string First_name { get; set; } = string.Empty;
        public string Last_name { get; set; } = string.Empty;
        public string User_name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone_number { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool Is_delete { get; set; }
        public bool Is_active { get; set; }
        public Guid? Created_by { get; set; }
        public DateTime? Created_date { get; set; }
        public Guid? Updated_by { get; set; }
        public DateTime? Updated_date { get; set; }
        public List<UserAccessRoleDetail> UserAccessRole { get; set; }
    }

    public class UserAccessRoleDetail
    {
        public Guid menu_type_id { get; set; }
        public string menu_type_name { get; set; }
        public Guid menu_permission_id { get; set; }
        public string slug { get; set; }
        public Guid? user_access_role_id { get; set; }
        public bool is_allowed { get; set; }

    }
}
