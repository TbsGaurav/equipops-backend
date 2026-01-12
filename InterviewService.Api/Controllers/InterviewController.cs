using InterviewService.Api.Helpers.EncryptionHelpers.Models;
using InterviewService.Api.Helpers.ResponseHelpers.Enums;
using InterviewService.Api.Helpers.ResponseHelpers.Handlers;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interview;
using InterviewService.Api.ViewModels.Request.Interview_transcript;
using InterviewService.Api.ViewModels.Response.Interview;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace InterviewService.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class InterviewController : ControllerBase
	{
		private readonly IInterviewService _interviewService;
		private readonly IInterviewTranscriptService _interviewTranscriptService;
		private readonly IInterviewerRepository _interview;
		private readonly ILogger<InterviewController> _iLogger;
		private readonly EncryptionSecretKey _encryptionSecretKey;

		public InterviewController(IInterviewService interviewService, IInterviewerRepository interview, ILogger<InterviewController> logger, IOptions<EncryptionSecretKey> encryptionOptions, IInterviewTranscriptService interviewTranscriptService)
		{
			_interviewService = interviewService;
			_iLogger = logger;
			_interview = interview;
			_encryptionSecretKey = encryptionOptions.Value;
			_interviewTranscriptService = interviewTranscriptService;
		}

		[HttpGet("list")]
		public async Task<IActionResult> GetInterviews(string? Search, int Length = 10, int Page = 1, string OrderColumn = "name", string OrderDirection = "Asc", bool? IsActive = null, Guid? OrganizationId = null)
		{
			_iLogger.LogInformation("API hit: GetInterviews | search={Search} page={Page} length={Length}",
				Search, Length, Page);

			// 🔹 Call Service
			var result = await _interviewService.GetInterviewsAsync(
				Search, Length, Page, OrderColumn, OrderDirection, IsActive, OrganizationId);

			_iLogger.LogInformation("API hit: GetInterviewsAsync. Search={Search}, Page={Page}, Length={Length}, IsActive={IsActive}", Search, Length, Page, IsActive);

			// 🔹 Failure
			if (!result.Success)
			{
				return BadRequest(ResponseHelper<string>.Error(
					result.Message ?? "Failed to retrieve interviews.",
					statusCode: StatusCodeEnum.BAD_REQUEST
				));
			}

			if (result.Data == null)
			{
				return NotFound(ResponseHelper<string>.Error(
					"Interviews not found.",
					statusCode: StatusCodeEnum.NOT_FOUND
				));
			}

			// 🔹 Success
			return Ok(ResponseHelper<InterviewListResponseViewModel>.Success(
				result.Message ?? "Interviews retrieved successfully.",
				result.Data
			));
		}

		[HttpGet("get-by-id")]
		public async Task<IActionResult> GetInterviewById(Guid Id)
		{
			_iLogger.LogInformation("API hit: GetInterviewById. Id={Id}", Id);
			// 🔹 Call Service
			var result = await _interviewService.GetInterviewByIdAsync(Id);

			_iLogger.LogInformation("API hit: GetInterviewByIdAsync. Id={Id}", Id);

			// 🔹 Failure
			if (!result.Success)
			{
				return BadRequest(ResponseHelper<string>.Error(
					result.Message ?? "Failed to retrieve interview.",
					statusCode: StatusCodeEnum.BAD_REQUEST
				));
			}

			if (result.Data == null)
			{
				return NotFound(ResponseHelper<string>.Error(
					"Interview not found.",
					statusCode: StatusCodeEnum.NOT_FOUND
				));
			}

			// 🔹 Success
			return Ok(ResponseHelper<InterviewByIdResponseViewModel>.Success(
				result.Message ?? "Interview retrieved successfully.",
				result.Data
			));
		}

		[HttpPost("create")]
		public async Task<IActionResult> CreateInterview([FromForm] InterviewCreateRequestViewModel request)
		{
			_iLogger.LogInformation("API hit: CreateInterview. Name={Name}", request.Name);
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
			var result = await _interviewService.CreateInterviewAsync(request);

			_iLogger.LogInformation(
				"Service response for Name={Name}: Success={Success}",
				request.Name,
				result.Success
			);

			// 🔹 Email Exists / Failure
			if (!result.Success)
			{
				return Conflict(ResponseHelper<string>.Error(
					result.Message ?? "Interview creation failed.",
					statusCode: StatusCodeEnum.CONFLICT_OCCURS
				));
			}

			// 🔹 Success
			if (result.Data == null)
			{
				return Conflict(ResponseHelper<string>.Error(
					"Interview creation failed.",
					statusCode: StatusCodeEnum.CONFLICT_OCCURS
				));
			}

			return Ok(ResponseHelper<InterviewCreateResponseViewModel>.Success("Interview created successfully.",
				result.Data
			));
		}

		[HttpPost("delete")]
		public async Task<IActionResult> DeleteInterview([FromBody] InterviewDeleteRequestViewModel request)
		{
			_iLogger.LogInformation("API hit: DeleteInterview. Id={Id}", request.Id);
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
			var result = await _interviewService.DeleteInterviewAsync(request);

			// 🔹 Success
			if (!result.Success || result.Data == null)
			{
				return NotFound(ResponseHelper<string>.Error(
					result.Message ?? "Interview deletion failed or interview not found.",
					statusCode: StatusCodeEnum.NOT_FOUND
				));
			}

			return Ok(ResponseHelper<InterviewListResponseViewModel>.Success(
				result.Message ?? "Interview deleted successfully.",
				result.Data
			));
		}

		[HttpGet("init")]
		public async Task<IActionResult> GetInterviewInit()
		{
			// 🔹 Call Service
			var result = await _interviewService.GetInterviewInitAsync();

			if (_encryptionSecretKey != null &&
				!string.IsNullOrWhiteSpace(_encryptionSecretKey.Secret))
			{
				Response.Headers["x-encryption-secret"] = _encryptionSecretKey.Secret;

				if (!Response.Headers.ContainsKey("Access-Control-Expose-Headers"))
					Response.Headers.Add("Access-Control-Expose-Headers", "x-encryption-secret");
			}
			return result;
		}

		[HttpPost("update")]
		public async Task<IActionResult> UpdateInterview([FromForm] InterviewUpdateRequestViewModel request)
		{
			_iLogger.LogInformation("API hit: UpdateInterview. Name={Name}", request.Name);
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
			var result = await _interviewService.UpdateInterviewAsync(request);

			_iLogger.LogInformation(
				"Service response for Name={Name}: Success={Success}",
				request.Name,
				result.Success
			);

			// 🔹 Email Exists / Failure
			if (!result.Success)
			{
				return Conflict(ResponseHelper<string>.Error(
					result.Message ?? "Interview updation failed.",
					statusCode: StatusCodeEnum.CONFLICT_OCCURS
				));
			}

			// 🔹 Success
			if (result.Data == null)
			{
				return Conflict(ResponseHelper<string>.Error(
					"Interview updation failed.",
					statusCode: StatusCodeEnum.CONFLICT_OCCURS
				));
			}

			return Ok(ResponseHelper<InterviewUpdateResponseViewModel>.Success("Interview updated successfully.",
				result.Data
			));
		}
		[AllowAnonymous]
		[HttpPost("create_token")]
		public async Task<IActionResult> GenerateInterviewToken([FromBody] InterviewTokenRequestViewModel request)
		{
			var result = await _interviewService.CreateInterviewTokenAsync(request);
			return Ok(ResponseHelper<string>.Success(
					  result.Message ?? "Interview token create successfully.",
					  result.Data
				  ));
		}
		[AllowAnonymous]
		[HttpPost("verify-token")]
		public async Task<IActionResult> VerifyInterviewToken([FromBody] VerifyTokenRequestViewModel model)
		{
			return await _interviewService.VerifyInterviewTokenAsync(model);
		}
		[AllowAnonymous]
		[HttpPost("call-register")]
		public async Task<IActionResult> RegisterCall([FromBody] CallRegisterRequestViewModel model)
		{
			var result = await _interviewService.RegisterCallAsync(model);
			return result;
		}
		[AllowAnonymous]
		[HttpPost("end-call")]
		public async Task<IActionResult> CreateInterviewTranscript([FromBody] InterviewTrasncriptCreateRequestViewModel request)
		{
			var response = await _interviewTranscriptService.CreateInterviewTranscriptAsync(request);
			return response;
		}

		[HttpGet("get-LLM-Key")]
		public async Task<IActionResult> GetLLMKey(Guid organizationId)
		{
			var result = await _interview.Get_retell_LLM_key(organizationId);
			return Ok(
				ResponseHelper<string>.Success("Token verified successfully",
				data: result)
			);

		}
	}
}
