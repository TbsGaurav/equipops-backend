using InterviewService.Api.Helpers.ResponseHelpers.Enums;
using InterviewService.Api.Helpers.ResponseHelpers.Handlers;
using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interviewer_setting;
using InterviewService.Api.ViewModels.Response.Interviewer_setting;

using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewerSettingController(ILogger<InterviewerSettingController> _iLogger, IInterviewerSettingService interviewerSettingService) : ControllerBase
    {
        [HttpGet("list")]
        public async Task<IActionResult> GetInterviewerSettings(string? Search, int Length = 10, int Page = 1, string OrderColumn = "first_name", string OrderDirection = "Asc", bool? IsActive = null)
        {
            _iLogger.LogInformation("API hit: GetInterviewerSettings | search={Search} page={Page} length={Length}",
                Search, Length, Page);
            // 🔹 Call Service
            var result = await interviewerSettingService.GetInterviewerSettingsAsync(
                Search, Length, Page, OrderColumn, OrderDirection, IsActive);

            _iLogger.LogInformation("API hit: GetInterviewerSettingsAsync. Search={Search}, Page={Page}, Length={Length}, IsActive={IsActive}", Search, Length, Page, IsActive);

            // 🔹 Failure
            if (!result.Success)
            {
                return BadRequest(ResponseHelper<string>.Error(
                    result.Message ?? "Failed to retrieve interviewer settings.",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            if (result.Data == null)
            {
                return NotFound(ResponseHelper<string>.Error(
                    "Interviewer settings not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            // 🔹 Success
            return Ok(ResponseHelper<InterviewerSettingListResponseViewModel>.Success(
                result.Message ?? "Interviewer settings retrieved successfully.",
                result.Data
            ));
        }
        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetInterviewerSettingById(Guid Id)
        {
            _iLogger.LogInformation("API hit: GetInterviewerSettingById. Id={Id}", Id);
            // 🔹 Call Service
            var result = await interviewerSettingService.GetInterviewerSettingByIdAsync(Id);

            _iLogger.LogInformation("API hit: GetInterviewerSettingByIdAsync. Id={Id}", Id);

            // 🔹 Failure
            if (!result.Success)
            {
                return BadRequest(ResponseHelper<string>.Error(
                    result.Message ?? "Failed to retrieve interviewer setting.",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            if (result.Data == null)
            {
                return NotFound(ResponseHelper<string>.Error(
                    "Interviewer setting not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            // 🔹 Success
            return Ok(ResponseHelper<InterviewerSettingData>.Success(
                result.Message ?? "Interviewer setting retrieved successfully.",
                result.Data
            ));
        }
        [HttpPost("create-update")]
        public async Task<IActionResult> CreateUpdateInterviewerSetting([FromBody] InterviewerSettingCreateUpdateRequestViewModel request)
        {
            _iLogger.LogInformation("API hit: CreateUpdateInterviewerSetting. Name={Name}", request.Name);

            // 🔹 Validate Model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _iLogger.LogWarning("Validation failed for Name={Name}", request.Name);

                return BadRequest(ResponseHelper<string>.Error(
                    "Validation failed",
                    errors: errors,
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            // 🔹 Call Service
            var result = await interviewerSettingService.CreateUpdateInterviewerSettingAsync(request);

            _iLogger.LogInformation(
                "Service response for Name={Name}: Success={Success}",
                request.Name,
                result.Success
            );

            // 🔹 Email Exists / Failure
            if (!result.Success)
            {
                return Conflict(ResponseHelper<string>.Error(
                    result.Message ?? "Interviewer setting creation failed.",
                    statusCode: StatusCodeEnum.CONFLICT_OCCURS
                ));
            }

            // 🔹 Success
            if (result.Data == null)
            {
                return Conflict(ResponseHelper<string>.Error(
                    "Interviewer setting creation failed.",
                    statusCode: StatusCodeEnum.CONFLICT_OCCURS
                ));
            }

            return Ok(ResponseHelper<InterviewerSettingCreateUpdateResponseViewModel>.Success(
                request.Id == null
                ? "Interviewer setting created successfully."
                : "Interviewer setting updated successfully.",
                result.Data
            ));
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteInterviewerSetting([FromBody] InterviewerSettingDeleteRequestViewModel request)
        {
            _iLogger.LogInformation("API hit: DeleteInterviewerSetting. Id={Id}", request.Id);

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
            var result = await interviewerSettingService.DeleteInterviewerSettingAsync(request);

            // 🔹 Success
            if (!result.Success || result.Data == null)
            {
                return NotFound(ResponseHelper<string>.Error(
                    result.Message ?? "Interviewer setting deletion failed or interview not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            return Ok(ResponseHelper<InterviewerSettingListResponseViewModel>.Success(
                result.Message ?? "Interviewer setting deleted successfully.",
                result.Data
            ));
        }
    }
}
