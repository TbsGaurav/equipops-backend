namespace OrganizationService.Api.ViewModels.Request.JobPost
{
    public class JobPost : JobTemplate
    {
        public Guid Id { get; set; }
        public Guid JobTemplateId { get; set; }
        public DateTime PublishedOn { get; set; }
        public DateTime ExpiresOn { get; set; }
        public bool IsActive { get; set; }
    }
}
