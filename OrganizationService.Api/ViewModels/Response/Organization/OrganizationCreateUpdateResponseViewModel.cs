namespace OrganizationService.Api.ViewModels.Response.Organization
{
    public class OrganizationCreateUpdateResponseViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Out_Email { get; set; } = string.Empty;
    }
}
