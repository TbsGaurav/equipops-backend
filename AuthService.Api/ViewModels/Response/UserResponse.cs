namespace AuthService.Api.ViewModels.Response
{
    public class UserResponse
    {
        public Guid user_id { get; set; }
        public string first_name { get; set; } = string.Empty;
        public string last_name { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string hash_password { get; set; } = string.Empty;

        // Read-only calculated property
        public string full_name => $"{last_name} {first_name}".Trim();
    }

    public class UserTokenResponse
    {
        public Guid user_id { get; set; }
        public string email { get; set; } = string.Empty;
        public DateTime? expired_at { get; set; }
    }
}
