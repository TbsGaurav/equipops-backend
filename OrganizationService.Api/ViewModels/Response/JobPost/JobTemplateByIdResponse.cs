using OrganizationService.Api.ViewModels.Request.JobPost;

namespace OrganizationService.Api.ViewModels.Response.JobPost
{
    public class JobTemplateByIdResponse
    {
        public JobTemplate Template { get; set; } = new JobTemplate();
    }
}
