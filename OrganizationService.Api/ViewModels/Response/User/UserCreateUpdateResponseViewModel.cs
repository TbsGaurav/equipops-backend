namespace OrganizationService.Api.ViewModels.Response.User
{
    public class UserCreateUpdateResponseViewModel
    {
        public Guid Id { get; set; }
        public string First_Name { get; set; } = string.Empty;
        public string Last_Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone_Number { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Guid? Organization_Id { get; set; }
        public string Role_Name { get; set; } = string.Empty;
        public Guid? Language_Id { get; set; }
    }
}
