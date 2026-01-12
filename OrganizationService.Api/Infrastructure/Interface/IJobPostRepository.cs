using Microsoft.AspNetCore.Mvc.Rendering;

using OrganizationService.Api.ViewModels.Request.JobPost;
using OrganizationService.Api.ViewModels.Response.JobPost;

namespace OrganizationService.Api.Infrastructure.Interface
{
    public interface IJobPostRepository
    {
        #region Job Template
        Task<JobTemplateListResponse> GetJobTemplatesAsync(
           string? search, int length, int page, string orderColumn, string orderDirection = "Asc", bool? IsActive = null);
        Task<JobTemplateByIdResponse> GetJobTemplateByIdAsync(Guid id);
        Task<JobTemplateCreateUpdateResponse> CreateUpdateJobTemplateAsync(JobTemplateCreateUpdateRequest request);
        Task DeleteJobTemplateAsync(JobTemplateDeleteRequest request);
        #endregion

        #region Job Post
        Task<JobPostListResponse> GetJobPostsAsync(string? search, int length, int page, string orderColumn, string orderDirection = "Asc", bool? isActive = null);
        Task<JobPostByIdResponse> GetJobPostByIdAsync(Guid id);
        Task<JobPostCreateResponse> PublishJobPostAsync(JobPostCreateRequest request);
        Task DeleteJobPostAsync(JobPostDeleteRequest request);
        Task<List<SelectListItem>> GetEmploymentTypeListAsync();
        #endregion
    }
}
