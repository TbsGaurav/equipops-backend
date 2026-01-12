using Common.Services.ViewModels.General;

using InterviewService.Api.Helpers.ResponseHelpers.Enums;
using InterviewService.Api.Helpers.ResponseHelpers.Handlers;
using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interview_Type;
using InterviewService.Api.ViewModels.Request.JobObjective;
using InterviewService.Api.ViewModels.Response.Interview;

using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewTypeController : ControllerBase
    {
        private readonly IInterviewTypeService _interviewTypeService;
        private readonly ILogger<InterviewTypeController> _iLogger;

        public InterviewTypeController(IInterviewTypeService interviewTypeService, ILogger<InterviewTypeController> logger)
        {
            _interviewTypeService = interviewTypeService;
            _iLogger = logger;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetInterviewTypes(string? Search, int Length = 10, int Page = 1, string OrderColumn = "interview_type", string OrderDirection = "Asc", bool? IsActive = null)
        {
            _iLogger.LogInformation("API hit: GetInterviewTypes | search={Search} page={Page} length={Length}",
                Search, Length, Page);

            // 🔹 Call Service
            var result = await _interviewTypeService.GetInterviewTypesAsync(
                Search, Length, Page, OrderColumn, OrderDirection, IsActive);

            _iLogger.LogInformation("API hit: GetInterviewTypesAsync. Search={Search}, Page={Page}, Length={Length}, IsActive={IsActive}", Search, Length, Page, IsActive);

            // 🔹 Failure
            if (!result.Success)
            {
                return BadRequest(ResponseHelper<string>.Error(
                    result.Message ?? "Failed to retrieve interview types.",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            if (result.Data == null)
            {
                return NotFound(ResponseHelper<string>.Error(
                    "Interview Types not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            // 🔹 Success
            return Ok(ResponseHelper<InterviewTypeListResponseViewModel>.Success(
                result.Message ?? "Interview Types retrieved successfully.",
                result.Data
            ));
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetInterviewTypeById(Guid Id)
        {
            _iLogger.LogInformation("API hit: GetInterviewTypeById. Id={Id}", Id);
            // 🔹 Call Service
            var result = await _interviewTypeService.GetInterviewTypeByIdAsync(Id);

            _iLogger.LogInformation("API hit: GetInterviewTypeByIdAsync. Id={Id}", Id);

            // 🔹 Failure
            if (!result.Success)
            {
                return BadRequest(ResponseHelper<string>.Error(
                    result.Message ?? "Failed to retrieve interview type.",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            if (result.Data == null)
            {
                return NotFound(ResponseHelper<string>.Error(
                    "Interview Type not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            // 🔹 Success
            return Ok(ResponseHelper<InterviewTypeByIdResponseViewModel>.Success(
                result.Message ?? "Interview Type retrieved successfully.",
                result.Data
            ));
        }

        [HttpPost("create-update")]
        public async Task<IActionResult> CreateUpdateInterviewType([FromBody] InterviewTypeCreateUpdateRequestViewModel request)
        {
            _iLogger.LogInformation("API hit: CreateUpdateInterviewType. Interview_Type={Interview_Type}", request.Interview_Type);
            // 🔹 Validate Model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _iLogger.LogWarning("Validation failed for Interview_Type={Interview_Type}", request.Interview_Type);

                return BadRequest(ResponseHelper<string>.Error(
                    "Validation failed",
                    errors: errors,
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            // 🔹 Call Service
            var result = await _interviewTypeService.CreateUpdateInterviewTypeAsync(request);

            _iLogger.LogInformation(
                "Service response for Interview_Type={Interview_Type}: Success={Success}",
                request.Interview_Type,
                result.Success
            );

            // 🔹 Email Exists / Failure
            if (!result.Success)
            {
                return Conflict(ResponseHelper<string>.Error(
                    result.Message ?? "Interview Type creation failed.",
                    statusCode: StatusCodeEnum.CONFLICT_OCCURS
                ));
            }

            // 🔹 Success
            if (result.Data == null)
            {
                return Conflict(ResponseHelper<string>.Error(
                    "Interview Type creation failed.",
                    statusCode: StatusCodeEnum.CONFLICT_OCCURS
                ));
            }

            return Ok(ResponseHelper<InterviewTypeCreateUpdateResponseViewModel>.Success(
                request.Id == null
                ? "Interview Type created successfully."
                : "Interview Type updated successfully.",
                result.Data
            ));
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteInterviewType([FromBody] InterviewTypeDeleteRequestViewModel request)
        {
            _iLogger.LogInformation("API hit: DeleteInterviewType. Id={Id}", request.Id);

            // 🔹 Failure
            if (request.Id == Guid.Empty)
            {
                _iLogger.LogWarning("Validation failed: Id is required. Id={Id}", request.Id);

                return BadRequest(ResponseHelper<string>.Error(
                    "Id is required",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            // 🔹 Call Service
            var result = await _interviewTypeService.DeleteInterviewTypeAsync(request);

            // 🔹 Success
            if (!result.Success || result.Data == null)
            {
                return NotFound(ResponseHelper<string>.Error(
                    result.Message ?? "Interview Type deletion failed or interview type not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            return Ok(ResponseHelper<InterviewTypeListResponseViewModel>.Success(
                result.Message ?? "Interview Type deleted successfully.",
                result.Data
            ));
        }

        [HttpPost("get-job-objective")]
        public async Task<IActionResult> GetJobObjective([FromBody] JobObjectiveRequestViewModel jobObjective)
        {
            _iLogger.LogInformation("API Hit: GetJobObjective | JobType={JobType}, WorkMode={WorkMode}, ExperienceYears={ExperienceYears}, job_Objective={job_Objective}",
                jobObjective.jobType, jobObjective.workMode, jobObjective.experienceYears, jobObjective.objective);


            // 🔹 Call Service
            var result = await _interviewTypeService
                .GetJobObjectiveAsync(jobObjective.jobType, jobObjective.workMode, jobObjective.experienceYears, jobObjective.objective);

            // 🔹 Service Failure
            if (!result.Success)
            {
                _iLogger.LogWarning(
                    "GetJobObjective failed | JobType={JobType}, WorkMode={WorkMode}, ExperienceYears={ExperienceYears},, job_Objective={job_Objective}, Message={Message}",
                    jobObjective.jobType, jobObjective.workMode, jobObjective.experienceYears, jobObjective.objective, result.Message
                );

                return BadRequest(ResponseHelper<string>.Error(
                    result.Message ?? "Failed to retrieve interview objective.",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            // 🔹 Data Not Found
            if (string.IsNullOrWhiteSpace(result.Data?.JobObjective))
            {
                _iLogger.LogWarning(
                    "GetJobObjective not found | JobType={JobType}, WorkMode={WorkMode}, ExperienceYears={ExperienceYears}, job_Objective={job_Objective}",
                    jobObjective.jobType, jobObjective.workMode, jobObjective.experienceYears, jobObjective.objective
                );

                return NotFound(ResponseHelper<string>.Error(
                    "Interview objective not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            // 🔹 Success
            _iLogger.LogInformation(
                "GetJobObjective success | JobType={JobType}, WorkMode={WorkMode}, ExperienceYears={ExperienceYears}, job_Objective={job_Objective}",
                jobObjective.jobType, jobObjective.workMode, jobObjective.experienceYears, jobObjective.objective
            );

            return Ok(ResponseHelper<JobObjectiveResponse>.Success(
                result.Message ?? "Interview objective retrieved successfully.",
                result.Data
            ));
        }
    }
}
