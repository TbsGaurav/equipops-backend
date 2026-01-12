namespace OrganizationService.Api.ViewModels.Request.User
{
    public class UserTokenRequestViewModel
    {
        public Guid User_Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Token_data { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Token_type { get; set; } = string.Empty;
        public string Ip_address { get; set; } = string.Empty;
        public DateTime Token_expiry { get; set; }
    }
}
