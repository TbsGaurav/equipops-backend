namespace AuthService.Api.ViewModels.Request
{
    public class LogoutRequest
    {
        public Guid? UserId { get; set; }
        public string Token { get; set; } = string.Empty;   // optional if needed
    }
}
