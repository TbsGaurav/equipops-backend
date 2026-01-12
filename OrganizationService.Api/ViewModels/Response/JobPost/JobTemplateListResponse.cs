using OrganizationService.Api.ViewModels.Request.JobPost;

namespace OrganizationService.Api.ViewModels.Response.JobPost
{

    public class JobTemplateListResponse
    {
        public int TotalNumbers { get; set; }
        public List<JobTemplate> jobTemplates { get; set; } = new();
    }
}
