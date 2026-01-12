using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using OrganizationService.Api.Helpers.EncryptionHelpers.Models;
using OrganizationService.Api.Helpers.ResponseHelpers.Enums;
using OrganizationService.Api.Helpers.ResponseHelpers.Handlers;
using OrganizationService.Api.Services.Interface;
using OrganizationService.Api.ViewModels.Request.OrganzationSetting;
using OrganizationService.Api.ViewModels.Response.OrganizationSetting;

namespace OrganizationService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationSettingController : ControllerBase
    {
        private readonly IOrganizationSettingService _organizationSettingService;
        private readonly ILogger<OrganizationSettingController> _iLogger;
        private readonly EncryptionSecretKey _encryptionSecretKey;
        private readonly IWebHostEnvironment _env;

        public OrganizationSettingController(
            IOrganizationSettingService organizationSettingService,
            ILogger<OrganizationSettingController> logger,
            IOptions<EncryptionSecretKey> encryptionSecretKey,
            IWebHostEnvironment env)
        {
            _organizationSettingService = organizationSettingService;
            _iLogger = logger;
            _encryptionSecretKey = encryptionSecretKey.Value;
            _env = env;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetOrganizationSettings()
        {
            _iLogger.LogInformation("API hit: GetOrganizationSettings");


            // 🔹 Call Service
            var result = await _organizationSettingService.GetOrganizationSettingsAsync();

            _iLogger.LogInformation("API hit: GetOrganizationSettingsAsync.");

            // 🔹 Failure
            if (!result.Success)
            {
                return BadRequest(ResponseHelper<string>.Error(
                    result.Message ?? "Failed to retrieve organization settings.",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            // 🔹 Add encryption secret only in Development
            Response.Headers["x-encryption-secret"] = _encryptionSecretKey.Secret;

            // Expose the header safely (avoid duplicate error)
            if (!Response.Headers.ContainsKey("Access-Control-Expose-Headers"))
                Response.Headers.Add("Access-Control-Expose-Headers", "x-encryption-secret");
            else
                Response.Headers.Append("Access-Control-Expose-Headers", "x-encryption-secret");

            if (result.Data.Count == 0)
            {
                return Ok(ResponseHelper<List<OrganizationSettingListResponseViewModel>>.Success(
                "Organization Settings not found.",
                result.Data
                ));
            }

            // 🔹 Success
            return Ok(ResponseHelper<List<OrganizationSettingListResponseViewModel>>.Success(
                result.Message ?? "Organization Settings retrieved successfully.",
                result.Data
            ));
        }

        [HttpGet("get-by-key")]
        public async Task<IActionResult> GetOrganizationSettingByKey(string Key)
        {
            _iLogger.LogInformation("API hit: GetOrganizationSettingByKey. Key={Key}", Key);


            // 🔹 Call Service
            var result = await _organizationSettingService.GetOrganizationSettingByKeyAsync(Key);

            _iLogger.LogInformation("API hit: GetOrganizationSettingByKeyAsync. Key={Key}", Key);

            // 🔹 Failure
            if (!result.Success)
            {
                return BadRequest(ResponseHelper<string>.Error(
                    result.Message ?? "Failed to retrieve organization setting.",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            if (result.Data == null)
            {
                return Ok(ResponseHelper<OrganizationSettingByKeyResponseViewModel>.Success(
                    "Organization Setting not found.",
                    result.Data
                ));
            }

            // 🔹 Success
            return Ok(ResponseHelper<OrganizationSettingByKeyResponseViewModel>.Success(
                result.Message ?? "Organization Setting retrieved successfully.",
                result.Data
            ));
        }

        [HttpPost("create-update")]
        public async Task<IActionResult> CreateUpdateOrganizationSetting([FromBody] OrganizationSettingCreateUpdateRequestViewModel request)
        {
            _iLogger.LogInformation("API hit: CreateUpdateOrganizationSetting. Key={Key}", request.Key);
            // 🔹 Validate Model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _iLogger.LogWarning("Validation failed for Key={Key}", request.Key);

                return BadRequest(ResponseHelper<string>.Error(
                    "Validation failed",
                    errors: errors,
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            // 🔹 Call Service
            var result = await _organizationSettingService.CreateUpdateOrganizationSettingAsync(request);

            _iLogger.LogInformation(
                "Service response for Id={Id}: Success={Success}",
                request.Key,
                result.Success
            );

            // 🔹 Email Exists / Failure					
            if (!result.Success || result.Data == null)
            {
                return Conflict(ResponseHelper<string>.Error(
                    result.Message ?? "Organization Setting creation failed.",
                    statusCode: StatusCodeEnum.CONFLICT_OCCURS
                ));
            }

            // 🔹 Success
            return Ok(ResponseHelper<OrganizationSettingCreateUpdateResponseViewModel>.Success(
                result.Message ?? "Organization Setting saved successfully.",
                result.Data
            ));
        }

        [HttpPost("generate-retell-llm")]
        public async Task<IActionResult> GenerateRetellLLMKey()
        {
            var result = await _organizationSettingService.Create_retell_llm_key();

            return Ok(ResponseHelper<OrganizationSettingCreateUpdateRequestViewModel>.Success(
                result.Message ?? "Retell LLM key generated successfully!",
                result.Data
            ));
        }
    }
}
