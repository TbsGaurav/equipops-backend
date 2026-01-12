namespace OrganizationService.Api.ViewModels.Response.User
{
    public class UserTokenResponseViewModel
    {
        public required Guid UserId { get; set; }
        public required string Email { get; set; }
    }
}
