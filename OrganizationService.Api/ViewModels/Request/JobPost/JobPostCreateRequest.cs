namespace OrganizationService.Api.ViewModels.Request.JobPost
{
    public class JobPostCreateRequest
    {
        public Guid JobTemplateId { get; set; }
        public int ValidMonths { get; set; } = 6;
    }
}
