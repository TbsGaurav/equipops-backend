using Common.Services.Helper;

using Microsoft.AspNetCore.Mvc.Rendering;

using OrganizationService.Api.ViewModels.Request.JobPost;
using OrganizationService.Api.ViewModels.Response.JobPost;

namespace OrganizationService.Api.Services.Interface
{
    public interface IJobPostService
    {
        #region Job Templates
        Task<ApiResponse<JobTemplateListResponse>> GetJobTemplatesAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null);
        Task<ApiResponse<JobTemplateByIdResponse>> GetJobTemplatesByIdAsync(Guid Id);
        Task<ApiResponse<JobTemplateCreateUpdateResponse>> CreateUpdateJobTemplateAsync(JobTemplateCreateUpdateRequest model);
        Task<ApiResponse<JobTemplateListResponse>> DeleteJobTemplateAsync(JobTemplateDeleteRequest model);
        Task<ApiResponse<List<SelectListItem>>> GetEmploymentTypeListAsync();
        #endregion

        #region Job Post
        Task<ApiResponse<JobPostListResponse>> GetJobPostAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null);
        Task<ApiResponse<JobPostByIdResponse>> GetJobPostByIdAsync(Guid Id);
        Task<ApiResponse<JobPostCreateResponse>> CreateUpdateJobPostAsync(JobPostCreateRequest model);
        Task<ApiResponse<JobPostListResponse>> DeleteJobPostAsync(JobPostDeleteRequest model);
        #endregion
    }
}
