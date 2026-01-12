namespace OrganizationService.Api.ViewModels.Response.JobPost
{
    public class JobTemplateCreateUpdateResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SubTitle { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
    }
}
