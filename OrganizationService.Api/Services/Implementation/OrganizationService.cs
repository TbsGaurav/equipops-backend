using Common.Services.Services.Interface;
using Common.Services.ViewModels.General;

using Microsoft.AspNetCore.Mvc;

using OrganizationService.Api.Helpers;
using OrganizationService.Api.Helpers.EncryptionHelpers.Handlers;
using OrganizationService.Api.Helpers.ResponseHelpers.Enums;
using OrganizationService.Api.Helpers.ResponseHelpers.Handlers;
using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.Services.Interface;
using OrganizationService.Api.ViewModels.Request.Organzation;
using OrganizationService.Api.ViewModels.Request.User;
using OrganizationService.Api.ViewModels.Response.Organization;
using OrganizationService.Api.ViewModels.Response.User;

using System.Text.Json;

namespace OrganizationService.Api.Services.Implementation
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IEmailService _emailService;
        private readonly EncryptionHelper _encryptionHelper;
        private readonly ILogger<OrganizationService> _logger;
        private readonly TokenHelper _tokenHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly string _gatewayBaseUrl;
        private readonly string _uploadProfilePath;
        private readonly IGeneralAIService _IGeneralAIService;
        private readonly string _openAIKey;

        public OrganizationService(
            IOrganizationRepository organizationRepository,
            ILogger<OrganizationService> logger,
            IEmailService emailService,
            EncryptionHelper encryptionHelper,
            TokenHelper tokenHelper,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IGeneralAIService generalAIService
            )
        {
            _organizationRepository = organizationRepository;
            _emailService = emailService;
            _encryptionHelper = encryptionHelper;
            _tokenHelper = tokenHelper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _uploadProfilePath = _configuration["ImageUploadSettings:ProfilePhotoUploadFolderPath"] ?? "Uploads/Profile";
            _gatewayBaseUrl = _configuration["ImageUploadSettings:BaseUrl"]
                              ?? string.Empty;
            _IGeneralAIService = generalAIService;
            _openAIKey = _configuration["OpenAI:Key"]
                             ?? string.Empty;
        }

        public async Task<IActionResult> GetOrganizationsAsync(string? Search, int Length = 10, int Page = 1, string OrderColumn = "name", string OrderDirection = "Asc", bool? IsActive = null)
        {
            try
            {
                // 🔹 Repository Call
                _logger.LogInformation("Calling OrganizationRepository.GetOrganizationsAsync.");

                var data = await _organizationRepository.GetOrganizationsAsync(Search, Length, Page, OrderColumn, OrderDirection, IsActive);

                // 🔹 Failure
                if (data == null)
                {
                    return new NotFoundObjectResult(ResponseHelper<string>.Error(
                        "Organizations not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    ));
                }

                // 🔹 Success
                _logger.LogInformation("Organization retrieved successfully.");

                return new OkObjectResult(ResponseHelper<OrganizationListResponseViewModel>.Success(
                    "Organizations retrieved successfully.",
                    data
                ));
            }
            catch (Exception ex)
            {
                // 🔹 Log Error
                _logger.LogError(ex, "Error retrieving organization list.");

                return new ObjectResult(ResponseHelper<string>.Error(
                    "An internal server error occurred.",
                    exception: ex,
                    statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                ));
            }
        }

        public async Task<IActionResult> GetOrganizationByIdAsync(Guid Id)
        {
            // 🔹 Validate Input
            if (Id == Guid.Empty)
            {
                _logger.LogWarning("Validation failed: Required fields are missing. Id={Id}", Id);

                return new BadRequestObjectResult(ResponseHelper<OrganizationListResponseViewModel>.Error(
                    "Id is required",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            // 🔹 Repository Call
            _logger.LogInformation("Calling OrganizationRepository.GetOrganizationByIdAsync.");

            var data = await _organizationRepository.GetOrganizationByIdAsync(Id);

            // 🔹 Failure
            if (data == null)
            {
                return new NotFoundObjectResult(ResponseHelper<OrganizationListResponseViewModel>.Error(
                    "Organization not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            // 🔹 Success
            _logger.LogInformation("Organization retrieved successfully.");

            return new OkObjectResult(ResponseHelper<OrganizationByIdResponseViewModel>.Success(
                "Organization retrieved successfully.",
                data
            ));
        }

        public async Task<IActionResult> CreateUpdateOrganizationAsync(OrganizationCreateUpdateRequestViewModel model)
        {
            // 🔹 Repository Call
            _logger.LogInformation("Calling OrganizationRepository.CreateUpdateOrganizationAsync for Email={Email}", model.Email);

            var data = await _organizationRepository.CreateUpdateOrganizationAsync(model);

            // 🔹 Email Exists / Failure
            if (data.UserId == Guid.Empty)
            {
                _logger.LogWarning("Organization creation/updation failed. No UserId returned. Email={Email}", model.Email);

                return new ConflictObjectResult(ResponseHelper<OrganizationCreateUpdateResponseViewModel>.Error(
                    "Email already exists.",
                    statusCode: StatusCodeEnum.CONFLICT_OCCURS
                ));
            }

            if (data == null)
            {
                return new ConflictObjectResult(ResponseHelper<string>.Error(
                    "Organization Setting creation failed.",
                    statusCode: StatusCodeEnum.CONFLICT_OCCURS
                ));
            }

            if (model.Id != null)
            {
                _logger.LogInformation("Organization updated successfully. Email={Email}", model.Email);

                return new OkObjectResult(ResponseHelper<OrganizationCreateUpdateResponseViewModel>.Success(
                    "Organization updated successfully.",
                    data
                ));
            }

            // 🔹 Generate Email Verification Token
            var payload = new { UserId = data.UserId, Email = data.Out_Email };
            var payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var encryptedToken = _encryptionHelper.EncryptForReact(payloadJson);

            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";

            // 🔹 Save token to DB
            await _tokenHelper.AddUserToken(new UserTokenRequestViewModel
            {
                User_Id = data.UserId,
                Email = data.Out_Email,
                Token = encryptedToken,
                Token_data = payloadJson,
                Token_type = "EmailVerification",
                Token_expiry = DateTime.UtcNow.AddHours(24),
                Ip_address = ipAddress
            });

            // 🔹 Send Welcome Email
            await _emailService.SendWelcomeEmailAsync(
                data.Out_Email,
                firstName: model.First_name
            );

            // 🔹 Send Verification Email
            var origin = _httpContextAccessor.HttpContext?.Request.Headers["Origin"].ToString();
            var verificationLink = $"{origin}/email-verify/{encryptedToken}";

            await _emailService.SendVerificationEmailAsync(data.Out_Email, verificationLink);

            // 🔹 Success
            _logger.LogInformation("Organization created successfully. Email={Email}", model.Email);

            return new OkObjectResult(ResponseHelper<OrganizationCreateUpdateResponseViewModel>.Success(
                "Organization created successfully.",
                 data
            ));
        }

        public async Task<IActionResult> VerifyRegistrationEmailAsync(EmailVerifyRequestViewModel model)
        {
            _logger.LogInformation("OrganizationService: VerifyRegistrationEmailAsync START. Token={Token}", model.Token);

            // 🔹 Decrypt Token and Extract User Info
            _logger.LogInformation("Decrypting email verification token...");

            // 🔹 Validate Token Presence
            if (string.IsNullOrEmpty(model.Token))
            {
                _logger.LogWarning("Email verification failed: Token is null or empty.");

                return new BadRequestObjectResult(ResponseHelper<EmailVerifyResponseViewModel>.Error(
                    "Token is required.",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            var decryptedJson = _encryptionHelper.DecryptFromReact(model.Token);

            // 🔹 Validate Decryption
            if (string.IsNullOrEmpty(decryptedJson))
            {
                _logger.LogWarning("Email verification failed: Token decryption returned null/empty.");

                return new NotFoundObjectResult(ResponseHelper<EmailVerifyResponseViewModel>.Error(
                    "Invalid or expired verification token.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            // 🔹 Parse token payload
            var userToken = JsonSerializer.Deserialize<UserTokenResponseViewModel>(decryptedJson);

            // 🔹 Validate Parsed Token
            if (userToken == null)
            {
                _logger.LogWarning("Email verification failed: Token JSON deserialization returned null.");

                return new NotFoundObjectResult(ResponseHelper<EmailVerifyResponseViewModel>.Error(
                    "Invalid or expired verification token.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            _logger.LogInformation("Token decrypted successfully. UserId={UserId}, Email={Email}", userToken.UserId, userToken.Email);

            // 🔹 Map to Entity
            var emailVerifyEntity = new EmailVerifyRequestViewModel
            {
                Token = _encryptionHelper.PadBase64(model.Token),
            };
            // 🔹 Repository Call
            _logger.LogInformation(
                "Calling OrganizationRepository.VerifyRegistrationEmailAsync for Token={Token}",
                model.Token
            );

            var response = await _organizationRepository.VerifyRegistrationEmailAsync(
                emailVerifyEntity,
                 userToken.Email,
                userToken.UserId
            );

            // 🔹Success log
            _logger.LogInformation("Email verification status={Status} for Email={Email}", response.Status, emailVerifyEntity.Token);

            return new OkObjectResult(ResponseHelper<EmailVerifyResponseViewModel>.Success(
                "Email verified successfully.",
                 response
            ));
        }

        public async Task<IActionResult> DeleteOrganizationAsync(OrganizationDeleteRequestViewModel model)
        {
            // 🔹 Validate Input
            if (model.Id == Guid.Empty)
            {
                _logger.LogWarning("Validation failed: Required fields are missing. Id={Id}", model.Id);

                return new BadRequestObjectResult(ResponseHelper<OrganizationListResponseViewModel>.Error(
                    "Id is required",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            // 🔹 Repository Call
            _logger.LogInformation("OrganizationService: DeleteOrganizationAsync START. with Id={Id}", model.Id);

            await _organizationRepository.DeleteOrganizationAsync(model);

            // 🔹 Fetch Updated List
            var data = await _organizationRepository.GetOrganizationsAsync(
                Search: null,
                Length: 10,
                Page: 1,
                OrderColumn: "name",
                OrderDirection: "Asc"
            );

            // 🔹 Failure
            if (data == null)
            {
                return new NotFoundObjectResult(ResponseHelper<OrganizationListResponseViewModel>.Error(
                    "Organization not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            // 🔹 Success
            _logger.LogInformation("Organization deleted successfully. Id={Id}", model.Id);

            return new OkObjectResult(ResponseHelper<OrganizationListResponseViewModel>.Success(
                "Organization deleted successfully.",
                data
            ));
        }

        public async Task<IActionResult> GetOrganizationProfileAsync(Guid id)
        {
            _logger.LogInformation("Service: GetOrganizationProfileAsync START for Id={Id}", id);

            var data = await _organizationRepository.GetOrganizationProfileAsync(id);

            if (data == null)
            {
                _logger.LogWarning("Service: No organization found for Id={Id}", id);

                return new NotFoundObjectResult(ResponseHelper<ProfileResponseViewModel>.Error(
                    "Organization profile not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            _logger.LogInformation("Service: Organization profile fetched successfully for Id={Id}", id);

            data.photo_url = GetProfilePhotoUrl(data.photo_url);

            return new OkObjectResult(ResponseHelper<ProfileResponseViewModel>.Success(
                "Organization profile retrieved successfully.",
                data
            ));
        }

        public async Task<IActionResult> UpdateProfileAsync(ProfileRequestViewModel model)
        {
            _logger.LogInformation(
                "Service: UpdateProfileAsync START | ProfileId={ProfileId}",
                model.id
            );

            try
            {
                // 🔹 Validate Input
                if (model.id == Guid.Empty)
                {
                    _logger.LogWarning("Validation failed: Required fields are missing. Id={Id}", model.id);

                    return new BadRequestObjectResult(ResponseHelper<ProfileResponseViewModel>.Error(
                        "Id is required",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    ));
                }

                /* -------------------- SAVE PHOTO -------------------- */
                if (model.photo != null && model.photo.Length > 0)
                {
                    _logger.LogInformation(
                        "Photo upload detected | ProfileId={ProfileId} | FileName={FileName} | Size={Size}",
                        model.id,
                        model.photo.FileName,
                        model.photo.Length
                    );

                    var uploadRoot = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        _uploadProfilePath,
                        model.id.ToString()
                    );

                    _logger.LogDebug(
                        "Resolved upload directory | Path={UploadRoot}",
                        uploadRoot
                    );

                    if (!Directory.Exists(uploadRoot))
                    {
                        Directory.CreateDirectory(uploadRoot);
                        _logger.LogInformation(
                            "Upload directory created | Path={UploadRoot}",
                            uploadRoot
                        );
                    }

                    /* -------------------- DELETE OLD PHOTO -------------------- */
                    var existingPhoto = await _organizationRepository
                        .GetExistingProfilePhotoAsync(model.id);

                    if (!string.IsNullOrWhiteSpace(existingPhoto))
                    {
                        var oldFilePath = Path.Combine(uploadRoot, Path.GetFileName(existingPhoto));

                        _logger.LogInformation(
                            "Existing profile photo found | ProfileId={ProfileId} | OldFile={OldFile}",
                            model.id,
                            oldFilePath
                        );

                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                            _logger.LogInformation(
                                "Old profile photo deleted | Path={OldFilePath}",
                                oldFilePath
                            );
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Old profile photo not found on disk | Path={OldFilePath}",
                                oldFilePath
                            );
                        }
                    }

                    /* -------------------- SAVE NEW PHOTO -------------------- */
                    var safeFileName = Path.GetFileName(model.photo.FileName);
                    var fileName = $"{model.id}_{safeFileName}";
                    var fullPath = Path.Combine(uploadRoot, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.photo.CopyToAsync(stream);
                    }

                    model.photo_url = $"Uploads/Profile/{model.id}/{fileName}";

                    _logger.LogInformation(
                        "New profile photo saved | ProfileId={ProfileId} | FilePath={PhotoPath}",
                        model.id,
                        model.photo_url
                    );
                }
                else
                {
                    var existingPhoto = await _organizationRepository
                        .GetExistingProfilePhotoAsync(model.id);
                    model.photo_url = existingPhoto ?? string.Empty;

                    _logger.LogInformation(
                        "No profile photo uploaded | ProfileId={ProfileId}",
                        model.id
                    );
                }

                /* -------------------- UPDATE PROFILE -------------------- */
                await _organizationRepository.UpdateProfileAsync(model);

                _logger.LogInformation(
                    "Profile update DB operation successful | ProfileId={ProfileId}",
                    model.id
                );

                /* -------------------- GET UPDATED PROFILE -------------------- */
                var result = await _organizationRepository.GetOrganizationProfileAsync(model.id);
                result.photo_url = GetProfilePhotoUrl(result.photo_url);

                return new OkObjectResult(
                    ResponseHelper<ProfileResponseViewModel>.Success(
                        "Profile updated successfully.",
                        result
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR in UpdateProfileAsync | ProfileId={ProfileId}",
                    model.id
                );

                throw; // Let middleware handle response
            }
        }

        public async Task<IActionResult> GetIndustryDeparmentAsync(string industry)
        {
            _logger.LogInformation("Service: GetIndustryDeparmentAsync START | Industry={Industry}", industry);

            if (string.IsNullOrWhiteSpace(industry))
            {
                _logger.LogWarning("Service: GetIndustryDeparmentAsync FAILED | Industry is null or empty");

                return new BadRequestObjectResult(
                    ResponseHelper<IndustryDepartmentResponse>.Error(
                        "Industry is required.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            if (string.IsNullOrWhiteSpace(_openAIKey))
            {
                _logger.LogWarning("Service: GetIndustryDeparmentAsync FAILED | OpenAI key not configured");

                return new NotFoundObjectResult(
                    ResponseHelper<IndustryDepartmentResponse>.Error(
                        "OpenAI key is not configured.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    )
                );
            }

            _logger.LogInformation("Service: Calling AI service for departments | Industry={Industry}", industry);

            var departments =
                await _IGeneralAIService.GetDepartmensByIndustryAsync(industry, _openAIKey);

            _logger.LogInformation("Service: GetIndustryDeparmentAsync SUCCESS | Industry={Industry} | DepartmentCount={Count}", industry, departments?.Departments?.Count ?? 0);

            return new OkObjectResult(
                ResponseHelper<IndustryDepartmentResponse>.Success(
                    "Departments retrieved successfully.",
                    departments
                )
            );
        }

        public async Task<IActionResult> GetAllOrganizations(string? search, int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "Asc", bool? isActive = null)
        {
            try
            {
                _logger.LogInformation(
                    "API Hit: GetAllOrganizations | Search={Search}, Page={Page}, Length={Length}, OrderColumn={OrderColumn}, OrderDirection={OrderDirection}, IsActive={IsActive}",
                    search, page, length, orderColumn, orderDirection, isActive
                );

                var data = await _organizationRepository.GetAllOrganizationsAsync(
                    search, length, page, orderColumn, orderDirection, isActive
                );

                if (data == null)
                {
                    _logger.LogWarning(
                        "No organizations found | Search={Search}, IsActive={IsActive}",
                        search, isActive
                    );

                    return new NotFoundObjectResult(ResponseHelper<string>.Error(
                        "Organizations not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    ));
                }

                _logger.LogInformation(
                    "Organizations retrieved successfully | Count={Count}",
                    data.OrganizationData?.Count ?? 0
                );

                return new OkObjectResult(ResponseHelper<OrganizationListResponseViewModel>.Success(
                    "Organizations retrieved successfully.",
                    data
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception in GetAllOrganizations | Search={Search}, Page={Page}, Length={Length}",
                    search, page, length
                );

                return new ObjectResult(ResponseHelper<string>.Error(
                    "An internal server error occurred.",
                    exception: ex,
                    statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                ));
            }
        }


        public async Task<IActionResult> GetAllOrganizationUsers(Guid orgId, string? search, int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "Asc", bool? isActive = null)
        {
            try
            {
                _logger.LogInformation(
                    "API Hit: GetAllOrganizationUsers | OrgId={OrgId}, Search={Search}, Page={Page}, Length={Length}, OrderColumn={OrderColumn}, OrderDirection={OrderDirection}, IsActive={IsActive}",
                    orgId, search, page, length, orderColumn, orderDirection, isActive
                );

                var data = await _organizationRepository.GetAllOrganizationUsers(
                    orgId, search, length, page, orderColumn, orderDirection, isActive
                );

                if (data == null)
                {
                    _logger.LogWarning(
                        "No users found for organization | OrgId={OrgId}, Search={Search}, IsActive={IsActive}",
                        orgId, search, isActive
                    );

                    return new NotFoundObjectResult(ResponseHelper<string>.Error(
                        "Users not found for the organization.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    ));
                }

                _logger.LogInformation(
                    "Organization users retrieved successfully | OrgId={OrgId}, Count={Count}",
                    orgId, data.UserData?.Count ?? 0
                );

                return new OkObjectResult(ResponseHelper<UserListResponseViewModel>.Success(
                    "Organization users retrieved successfully.",
                    data
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception in GetAllOrganizationUsers | OrgId={OrgId}, Search={Search}, Page={Page}, Length={Length}",
                    orgId, search, page, length
                );

                return new ObjectResult(ResponseHelper<string>.Error(
                    "An internal server error occurred.",
                    exception: ex,
                    statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                ));
            }
        }

        public async Task<IActionResult> UpdateOrganizationStatusAsync(Guid organizationId, string action)
        {
            _logger.LogInformation(
                "Service: UpdateOrganizationStatus | OrgId={OrgId}, Action={Action}",
                organizationId, action
            );

            var result = await _organizationRepository.UpdateOrganizationStatusAsync(organizationId, action);

            if (result.Success)
            {
                return new OkObjectResult(ResponseHelper<string>.Success(result.Message));
            }
            else
            {
                _logger.LogWarning(result.Message);

                return new NotFoundObjectResult(ResponseHelper<string>.Error(
                    result.Message,
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }
        }

        public async Task<IActionResult> GetOrganizationsByStatus(string? search, int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "Asc", int? status = 0)
        {
            try
            {
                _logger.LogInformation("API Hit: GetOrganizationsByStatus | Search={Search}, Page={Page}, Length={Length}, OrderColumn={OrderColumn}, OrderDirection={OrderDirection}, status={status}", search, page, length, orderColumn, orderDirection, status);

                var data = await _organizationRepository.GetOrganizationsByStatus(search, length, page, orderColumn, orderDirection, status);

                if (data == null)
                {
                    _logger.LogWarning(
                        "No organizations found | Search={Search}, status={status}",
                        search, status
                    );

                    return new NotFoundObjectResult(ResponseHelper<string>.Error(
                        "Organizations not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    ));
                }

                _logger.LogInformation(
                    "Organizations retrieved successfully | Count={Count}",
                    data.OrganizationData?.Count ?? 0
                );

                return new OkObjectResult(ResponseHelper<OrganizationListResponseViewModel>.Success(
                    "Organizations retrieved successfully.",
                    data
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception in GetOrganizationsByStatus | Search={Search}, Page={Page}, Length={Length}",
                    search, page, length
                );

                return new ObjectResult(ResponseHelper<string>.Error(
                    "An internal server error occurred.",
                    exception: ex,
                    statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                ));
            }
        }

        private string GetProfilePhotoUrl(string? photo_url)
        {
            if (string.IsNullOrWhiteSpace(photo_url))
            {
                _logger.LogDebug("GetProfilePhotoUrl: Empty photo_url");
                return string.Empty;
            }

            var publicPath = $"org/{photo_url}".Replace("\\", "/");
            var fullUrl = $"{_gatewayBaseUrl.TrimEnd('/')}/{publicPath}";

            _logger.LogDebug(
                "Resolved profile photo URL | Input={PhotoUrl} | Output={FullUrl}",
                photo_url,
                fullUrl
            );

            return fullUrl;
        }

    }
}
