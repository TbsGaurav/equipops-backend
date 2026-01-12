using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using OrganizationService.Api.Helpers.ResponseHelpers.Enums;
using OrganizationService.Api.Helpers.ResponseHelpers.Handlers;
using OrganizationService.Api.Services.Interface;
using OrganizationService.Api.ViewModels.Request.JobPost;
using OrganizationService.Api.ViewModels.Response.JobPost;

namespace OrganizationService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobPostController : ControllerBase
    {
        private readonly IJobPostService _jobPostService;
        private readonly ILogger<JobPostController> _iLogger;

        public JobPostController(IJobPostService jobPostService, ILogger<JobPostController> logger)
        {
            _jobPostService = jobPostService;
            _iLogger = logger;
        }

        #region Job Templates
        [HttpGet("jobTemplate/list")]
        public async Task<IActionResult> GetJobTemplates(string? Search, int Length = 10, int Page = 1, string OrderColumn = "title", string OrderDirection = "Asc", bool? IsActive = null)
        {
            _iLogger.LogInformation("API hit: GetJobTemplates | search={Search} page={Page} length={Length}", Search, Length, Page);

            var result = await _jobPostService.GetJobTemplatesAsync(
                    Search, Length, Page, OrderColumn, OrderDirection, IsActive);

            _iLogger.LogInformation("API hit: GetJobTemplates. Search={Search}, Page={Page}, Length={Length}, IsActive={IsActive}", Search, Length, Page, IsActive);

            if (!result.Success)
                return BadRequest(ResponseHelper<string>.Error(result.Message ?? "Failed to retrieve job templates.", statusCode: StatusCodeEnum.BAD_REQUEST));

            if (result.Data == null)
                return NotFound(ResponseHelper<string>.Error("Job templates not found.", statusCode: StatusCodeEnum.NOT_FOUND));

            return Ok(ResponseHelper<JobTemplateListResponse>.Success(result.Message ?? "Job templates retrieved successfully.", result.Data));
        }

        [HttpGet("jobTemplate/get-by-id")]
        public async Task<IActionResult> GetJobTemplatesById(Guid Id)
        {
            _iLogger.LogInformation("API hit: GetJobPostById. Id={Id}", Id);

            var result = await _jobPostService.GetJobTemplatesByIdAsync(Id);

            _iLogger.LogInformation("API hit: GetJobPostByIdAsync. Id={Id}", Id);

            if (!result.Success)
                return BadRequest(ResponseHelper<string>.Error(result.Message ?? "Failed to retrieve job Template.", statusCode: StatusCodeEnum.BAD_REQUEST));

            if (result.Data == null)
                return NotFound(ResponseHelper<string>.Error("Job Template not found.", statusCode: StatusCodeEnum.NOT_FOUND));

            return Ok(ResponseHelper<JobTemplateByIdResponse>.Success(result.Message ?? "Job Template retrieved successfully.", result.Data));
        }

        [HttpPost("jobTemplate/create-update")]
        public async Task<IActionResult> CreateUpdateJobTemplate([FromBody] JobTemplateCreateUpdateRequest request)
        {
            _iLogger.LogInformation("API hit: CreateUpdateJobTemplate. Title={Title}", request.Title);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _iLogger.LogWarning("Validation failed for Title={Title}", request.Title);
                return BadRequest(ResponseHelper<string>.Error("Validation failed", errors: errors, statusCode: StatusCodeEnum.BAD_REQUEST));
            }

            var result = await _jobPostService.CreateUpdateJobTemplateAsync(request);

            _iLogger.LogInformation("Service response for Title={Title}: Success={Success}", request.Title, result.Success);

            if (!result.Success)
                return Conflict(ResponseHelper<string>.Error(result.Message ?? "Job Template creation failed.", statusCode: StatusCodeEnum.CONFLICT_OCCURS));

            if (result.Data == null)
                return Conflict(ResponseHelper<string>.Error("Job Template creation failed.", statusCode: StatusCodeEnum.CONFLICT_OCCURS));

            return Ok(ResponseHelper<JobTemplateCreateUpdateResponse>.Success(
                request.Id == null ? "Job Template created successfully." : "Job Template updated successfully.",
                result.Data
            ));
        }

        [HttpPost("jobTemplate/delete")]
        public async Task<IActionResult> DeleteJobTemplate([FromBody] JobTemplateDeleteRequest request)
        {
            _iLogger.LogInformation("API hit: DeleteJobTemplate. Id={Id}", request.Id);

            if (request.Id == Guid.Empty)
            {
                _iLogger.LogWarning("Validation failed: Id is required. Id={Id}", request.Id);
                return BadRequest(ResponseHelper<string>.Error("Id is required", statusCode: StatusCodeEnum.BAD_REQUEST));
            }

            var result = await _jobPostService.DeleteJobTemplateAsync(request);

            if (!result.Success || result.Data == null)
                return NotFound(ResponseHelper<string>.Error(result.Message ?? "Job Template deletion failed or job post not found.", statusCode: StatusCodeEnum.NOT_FOUND));

            return Ok(ResponseHelper<JobTemplateListResponse>.Success(result.Message ?? "Job Template deleted successfully.", result.Data));
        }

        [HttpGet("jobTemplate/employment-type-list")]
        public async Task<IActionResult> GetEmploymentTypeList()
        {
            _iLogger.LogInformation("API hit: GetEmploymentTypeList");

            var result = await _jobPostService.GetEmploymentTypeListAsync();

            if (!result.Success)
            {
                _iLogger.LogWarning("GetEmploymentTypeList failed: {Message}", result.Message);

                return BadRequest(
                    ResponseHelper<string>.Error(
                        result.Message ?? "Failed to retrieve employment types.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            // For list APIs, always return 200 with empty list
            var data = result.Data ?? [];

            _iLogger.LogInformation("GetEmploymentTypeList success. Count={Count}", data.Count);

            return Ok(
                ResponseHelper<List<SelectListItem>>.Success(
                    result.Message ?? "Employment types retrieved successfully.",
                    data
                )
            );
        }

        #endregion

        #region Job Post
        [HttpGet("jobPost/list")]
        public async Task<IActionResult> GetJobPostAsync(string? Search, int Length = 10, int Page = 1, string OrderColumn = "title", string OrderDirection = "Asc", bool? IsActive = null)
        {
            _iLogger.LogInformation("API hit: GetJobPostAsync | search={Search} page={Page} length={Length}", Search, Length, Page);

            var result = await _jobPostService.GetJobPostAsync(
                    Search, Length, Page, OrderColumn, OrderDirection, IsActive);

            _iLogger.LogInformation("API hit: GetJobPostAsync. Search={Search}, Page={Page}, Length={Length}, IsActive={IsActive}", Search, Length, Page, IsActive);

            if (!result.Success)
                return BadRequest(ResponseHelper<string>.Error(result.Message ?? "Failed to retrieve job post.", statusCode: StatusCodeEnum.BAD_REQUEST));

            if (result.Data == null)
                return NotFound(ResponseHelper<string>.Error("Job post not found.", statusCode: StatusCodeEnum.NOT_FOUND));

            return Ok(ResponseHelper<JobPostListResponse>.Success(result.Message ?? "Job post retrieved successfully.", result.Data));
        }

        [HttpGet("jobPost/get-by-id")]
        public async Task<IActionResult> GetJobPostById(Guid Id)
        {
            _iLogger.LogInformation("API hit: GetJobPostById. Id={Id}", Id);

            var result = await _jobPostService.GetJobPostByIdAsync(Id);

            _iLogger.LogInformation("API hit: GetJobPostById. Id={Id}", Id);

            if (!result.Success)
                return BadRequest(ResponseHelper<string>.Error(result.Message ?? "Failed to retrieve job post.", statusCode: StatusCodeEnum.BAD_REQUEST));

            if (result.Data == null)
                return NotFound(ResponseHelper<string>.Error("Job post not found.", statusCode: StatusCodeEnum.NOT_FOUND));

            return Ok(ResponseHelper<JobPostByIdResponse>.Success(result.Message ?? "Job Template retrieved successfully.", result.Data));
        }

        [HttpPost("jobPost/create-update")]
        public async Task<IActionResult> CreateUpdateJobPost([FromBody] JobPostCreateRequest request)
        {
            _iLogger.LogInformation("API hit: CreateUpdateJobPost. JobTemplateId={JobTemplateId}", request.JobTemplateId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _iLogger.LogWarning("API hit: CreateUpdateJobPost. JobTemplateId={JobTemplateId}", request.JobTemplateId);
                return BadRequest(ResponseHelper<string>.Error("Validation failed", errors: errors, statusCode: StatusCodeEnum.BAD_REQUEST));
            }

            var result = await _jobPostService.CreateUpdateJobPostAsync(request);

            _iLogger.LogInformation("Service response for JobTemplateId={JobTemplateId}: Success={Success}", request.JobTemplateId, result.Success);

            if (!result.Success)
                return Conflict(ResponseHelper<string>.Error(result.Message ?? "Job Post creation failed.", statusCode: StatusCodeEnum.CONFLICT_OCCURS));

            if (result.Data == null)
                return Conflict(ResponseHelper<string>.Error("Job Post creation failed.", statusCode: StatusCodeEnum.CONFLICT_OCCURS));

            return Ok(ResponseHelper<JobPostCreateResponse>.Success("Job Post publised successfully.",
                result.Data
            ));
        }

        [HttpPost("jobPost/delete")]
        public async Task<IActionResult> DeleteJobPost([FromBody] JobPostDeleteRequest request)
        {
            _iLogger.LogInformation("API hit: DeleteJobPost. Id={Id}", request.Id);

            if (request.Id == Guid.Empty)
            {
                _iLogger.LogWarning("Validation failed: Id is required. Id={Id}", request.Id);
                return BadRequest(ResponseHelper<string>.Error("Id is required", statusCode: StatusCodeEnum.BAD_REQUEST));
            }

            var result = await _jobPostService.DeleteJobPostAsync(request);

            if (!result.Success || result.Data == null)
                return NotFound(ResponseHelper<string>.Error(result.Message ?? "Job Post deletion failed or job post not found.", statusCode: StatusCodeEnum.NOT_FOUND));

            return Ok(ResponseHelper<JobPostListResponse>.Success(result.Message ?? "Job Post deleted successfully.", result.Data));
        }
        #endregion
    }
}

