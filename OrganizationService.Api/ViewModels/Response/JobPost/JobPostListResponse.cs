namespace OrganizationService.Api.ViewModels.Response.JobPost
{
    public class JobPostListResponse
    {
        public int TotalNumbers { get; set; }
        public List<Request.JobPost.JobPost> JobPosts { get; set; } = new();
    }
}
