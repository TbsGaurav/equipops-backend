namespace AuthService.Api.ViewModels.Request
{
    public class ChangePasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
