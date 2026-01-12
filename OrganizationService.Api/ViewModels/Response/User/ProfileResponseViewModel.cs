namespace OrganizationService.Api.ViewModels.Response.User
{
    public class ProfileResponseViewModel
    {
        public Guid id { get; set; }
        public string first_name { get; set; } = string.Empty;
        public string last_name { get; set; } = string.Empty;
        public string full_name { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string phone_no { get; set; } = string.Empty;
        public string location { get; set; } = string.Empty;
        public string job_title { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
        public string photo_url { get; set; } = string.Empty;
        public IFormFile? photo { get; set; }
    }
}
