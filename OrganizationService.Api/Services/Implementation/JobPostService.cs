using Common.Services.Helper;

using Microsoft.AspNetCore.Mvc.Rendering;

using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.Services.Interface;
using OrganizationService.Api.ViewModels.Request.JobPost;
using OrganizationService.Api.ViewModels.Response.JobPost;

namespace OrganizationService.Api.Services.Implementation
{
    public class JobPostService : IJobPostService
    {
        private readonly IJobPostRepository _jobPostRepository;
        private readonly ILogger<JobPostService> _logger;

        public JobPostService(
            IJobPostRepository jobPostRepository,
            ILogger<JobPostService> logger
            )
        {
            _jobPostRepository = jobPostRepository;
            _logger = logger;
        }

        #region Job Templates
        public async Task<ApiResponse<JobTemplateListResponse>> GetJobTemplatesAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null)
        {
            // 🔹 Repository Call
            _logger.LogInformation("Calling JobTemplateRepository.GetJobTemplatesAsync.");

            var data = await _jobPostRepository.GetJobTemplatesAsync(Search, Length, Page, OrderColumn, OrderDirection, IsActive);

            // 🔹 Success
            _logger.LogInformation("Job Templates retrieved successfully.");

            return new ApiResponse<JobTemplateListResponse>
            {
                Success = true,
                Message = "Job Templates retrieved successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<JobTemplateByIdResponse>> GetJobTemplatesByIdAsync(Guid Id)
        {
            // 🔹 Repository Call
            _logger.LogInformation("Calling JobPostRepository.GetJobTemplatesByIdAsync.");

            var data = await _jobPostRepository.GetJobTemplateByIdAsync(Id);

            // 🔹 Success
            _logger.LogInformation("Job Templates retrieved successfully.");

            return new ApiResponse<JobTemplateByIdResponse>
            {
                Success = true,
                Message = "Job Templates retrieved successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<JobTemplateCreateUpdateResponse>> CreateUpdateJobTemplateAsync(JobTemplateCreateUpdateRequest model)
        {
            _logger.LogInformation("JobPostService: CreateUpdateJobTemplateAsync START. Title={Title}", model.Title);

            // 🔹 Validate Input
            if (model.Id == Guid.Empty || string.IsNullOrEmpty(model.Title))
            {
                _logger.LogWarning("Validation failed: Required fields are missing. Title={Title}", model.Title);

                return new ApiResponse<JobTemplateCreateUpdateResponse>
                {
                    Success = false,
                    Message = "Job Template Id and Title are required."
                };
            }

            // 🔹 Repository Call
            _logger.LogInformation("Calling JobPostRepository.CreateUpdateJobTemplateAsync for Title={Title}", model.Title);

            var data = await _jobPostRepository.CreateUpdateJobTemplateAsync(model);

            if (data == null || data.Id == Guid.Empty)
            {
                _logger.LogWarning("Job Templates creation/updation failed. No Id returned. Title={Title}", model.Title);

                return new ApiResponse<JobTemplateCreateUpdateResponse>
                {
                    Success = false,
                    Message = "Job Template creation/updation failed.",
                    Data = data
                };
            }

            if (data.Id != null && data.Id != Guid.Empty)
            {
                _logger.LogInformation("Job Template updated successfully. Title={Title}", model.Title);

                return new ApiResponse<JobTemplateCreateUpdateResponse>
                {
                    Success = true,
                    Message = "Job Template updated successfully.",
                    Data = data
                };
            }


            _logger.LogWarning("Job Templates creation/updation failed. No Id returned. Title={Title}", model.Title);

            return new ApiResponse<JobTemplateCreateUpdateResponse>
            {
                Success = false,
                Message = "Job Template creation/updation failed.",
                Data = data
            };
        }

        public async Task<ApiResponse<JobTemplateListResponse>> DeleteJobTemplateAsync(JobTemplateDeleteRequest model)
        {
            // 🔹 Repository Call
            _logger.LogInformation("JobPostService: DeleteJobTemplateAsync START. with Id={Id}", model.Id);

            await _jobPostRepository.DeleteJobTemplateAsync(model);

            // 🔹 Fetch Updated List
            var data = await _jobPostRepository.GetJobTemplatesAsync(
                search: null,
                length: 10,
                page: 1,
                orderColumn: "name",
                orderDirection: "Asc"
            );

            // 🔹 Success
            _logger.LogInformation("Job Templates deleted successfully. Id={Id}", model.Id);

            return new ApiResponse<JobTemplateListResponse>
            {
                Success = true,
                Message = "Job Templates deleted successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<List<SelectListItem>>> GetEmploymentTypeListAsync()
        {
            _logger.LogInformation("Service: GetEmploymentTypeListAsync started.");

            var data = await _jobPostRepository.GetEmploymentTypeListAsync();

            if (data == null || !data.Any())
            {
                _logger.LogWarning("Service: No employment types found.");

                return new ApiResponse<List<SelectListItem>>
                {
                    Success = true,
                    Message = "No employment types available.",
                    Data = new List<SelectListItem>()
                };
            }

            _logger.LogInformation("Service: Retrieved {Count} employment types.", data.Count);

            return new ApiResponse<List<SelectListItem>>
            {
                Success = true,
                Message = "Employment type list retrieved successfully.",
                Data = data
            };
        }
        #endregion

        #region Job Post
        public async Task<ApiResponse<JobPostListResponse>> GetJobPostAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null)
        {
            // 🔹 Repository Call
            _logger.LogInformation("Calling JobTemplateRepository.GetJobPostsAsync.");

            var data = await _jobPostRepository.GetJobPostsAsync(Search, Length, Page, OrderColumn, OrderDirection, IsActive);

            // 🔹 Success
            _logger.LogInformation("Job Post retrieved successfully.");

            return new ApiResponse<JobPostListResponse>
            {
                Success = true,
                Message = "Job Post retrieved successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<JobPostByIdResponse>> GetJobPostByIdAsync(Guid Id)
        {
            // 🔹 Repository Call
            _logger.LogInformation("Calling JobPostRepository.GetJobPostByIdAsync.");

            var data = await _jobPostRepository.GetJobPostByIdAsync(Id);

            // 🔹 Success
            _logger.LogInformation("Job Post retrieved successfully.");

            return new ApiResponse<JobPostByIdResponse>
            {
                Success = true,
                Message = "Job Post retrieved successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<JobPostCreateResponse>> CreateUpdateJobPostAsync(JobPostCreateRequest model)
        {
            _logger.LogInformation("JobPostService: CreateUpdateJobPostAsync START. JobTemplateId={JobTemplateId}", model.JobTemplateId);

            // 🔹 Validate Input
            if (model.JobTemplateId == Guid.Empty || model.JobTemplateId == null)
            {
                _logger.LogWarning("Validation failed: Required fields are missing. Title={Title}", model.JobTemplateId);

                return new ApiResponse<JobPostCreateResponse>
                {
                    Success = false,
                    Message = "Job Template Id is required."
                };
            }

            // 🔹 Repository Call
            _logger.LogInformation("Calling JobPostRepository.CreateUpdateJobPostAsync for Title={JobTemplateId}", model.JobTemplateId);

            var data = await _jobPostRepository.PublishJobPostAsync(model);

            if (data == null || data.JobPostId == Guid.Empty)
            {
                _logger.LogWarning("Job Post creation/updation failed. No Id returned. JobPostId={JobPostId}", data?.JobPostId);

                return new ApiResponse<JobPostCreateResponse>
                {
                    Success = false,
                    Message = "Job Post creation/updation failed.",
                    Data = data
                };
            }

            if (data.JobPostId != null && data.JobPostId != Guid.Empty)
            {
                _logger.LogInformation("Job Post published successfully. JobPost={JobPost}", data.JobPostId);

                return new ApiResponse<JobPostCreateResponse>
                {
                    Success = true,
                    Message = "Job Post published successfully.",
                    Data = data
                };
            }


            _logger.LogWarning("Job Post creation/updation failed. No Id returned. JobPostId={JobPostId}", data.JobPostId);

            return new ApiResponse<JobPostCreateResponse>
            {
                Success = false,
                Message = "Job Post creation/updation failed.",
                Data = data
            };
        }

        public async Task<ApiResponse<JobPostListResponse>> DeleteJobPostAsync(JobPostDeleteRequest model)
        {
            // 🔹 Repository Call
            _logger.LogInformation("JobPostService: DeleteJobPostAsync START. with Id={Id}", model.Id);

            await _jobPostRepository.DeleteJobPostAsync(model);

            // 🔹 Fetch Updated List
            var data = await _jobPostRepository.GetJobPostsAsync(
                search: null,
                length: 10,
                page: 1,
                orderColumn: "name",
                orderDirection: "Asc"
            );

            // 🔹 Success
            _logger.LogInformation("Job Post deleted successfully. Id={Id}", model.Id);

            return new ApiResponse<JobPostListResponse>
            {
                Success = true,
                Message = "Job Post deleted successfully.",
                Data = data
            };
        }
        #endregion
    }
}
