using Common.Services.Services.Interface;
using Common.Services.ViewModels.General;

using InterviewService.Api.Helpers.ResponseHelpers.Models;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interview_Type;
using InterviewService.Api.ViewModels.Response.Interview;

namespace InterviewService.Api.Services.Implementation
{
    public class InterviewTypeService : IInterviewTypeService
    {
        private readonly IInterviewTypeRepository _interviewTypeRepository;
        private readonly IInterviewerRepository _interviewerRepository;
        private readonly ILogger<InterviewTypeService> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IGeneralAIService _iGeneralAIService;

        public InterviewTypeService(
            IInterviewTypeRepository interviewTypeRepository,
            ILogger<InterviewTypeService> logger, IInterviewerRepository interviewerRepository, IHttpContextAccessor contextAccessor, IGeneralAIService iGeneralAIService
            )
        {
            _interviewTypeRepository = interviewTypeRepository;
            _logger = logger;
            _interviewerRepository = interviewerRepository;
            _contextAccessor = contextAccessor;
            _iGeneralAIService = iGeneralAIService;
        }

        public async Task<ApiResponse<InterviewTypeListResponseViewModel>> GetInterviewTypesAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null)
        {
            // 🔹 Repository Call
            _logger.LogInformation("Calling InterviewTypeRepository.GetInterviewTypesAsync.");

            var data = await _interviewTypeRepository.GetInterviewTypesAsync(Search, Length, Page, OrderColumn, OrderDirection, IsActive);

            // 🔹 Success
            _logger.LogInformation("Interview Types retrieved successfully.");

            return new ApiResponse<InterviewTypeListResponseViewModel>
            {
                Success = true,
                Message = "Interview Types retrieved successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<InterviewTypeByIdResponseViewModel>> GetInterviewTypeByIdAsync(Guid Id)
        {
            // 🔹 Repository Call
            _logger.LogInformation("Calling InterviewTypeRepository.GetInterviewTypeByIdAsync.");

            var data = await _interviewTypeRepository.GetInterviewTypeByIdAsync(Id);

            // 🔹 Success
            _logger.LogInformation("Interview Type retrieved successfully.");

            return new ApiResponse<InterviewTypeByIdResponseViewModel>
            {
                Success = true,
                Message = "Interview Type retrieved successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<InterviewTypeCreateUpdateResponseViewModel>> CreateUpdateInterviewTypeAsync(InterviewTypeCreateUpdateRequestViewModel model)
        {
            _logger.LogInformation("InterviewTypeService: CreateUpdateInterviewTypeAsync START. Interview_Type={Interview_Type}", model.Interview_Type);

            // 🔹 Validate Input
            if (string.IsNullOrEmpty(model.Interview_Type))
            {
                _logger.LogWarning("Validation failed: Required fields are missing. Interview_Type={Interview_Type}", model.Interview_Type);

                return new ApiResponse<InterviewTypeCreateUpdateResponseViewModel>
                {
                    Success = false,
                    Message = "Interview Type is required."
                };
            }

            // 🔹 Repository Call
            _logger.LogInformation("Calling InterviewTypeRepository.CreateUpdateInterviewTypeAsync for Interview_Type={Interview_Type}", model.Interview_Type);

            var data = await _interviewTypeRepository.CreateUpdateInterviewTypeAsync(model);

            if (data == null || data.Id == Guid.Empty)
            {
                _logger.LogWarning("Interview Type creation/updation failed. No Id returned. Interview_Type={Interview_Type}", model.Interview_Type);

                return new ApiResponse<InterviewTypeCreateUpdateResponseViewModel>
                {
                    Success = false,
                    Message = "Interview Type creation/updation failed.",
                    Data = data
                };
            }

            if (model.Id != null)
            {
                _logger.LogInformation("Interview Type updated successfully. Interview_Type={Interview_Type}", model.Interview_Type);

                return new ApiResponse<InterviewTypeCreateUpdateResponseViewModel>
                {
                    Success = true,
                    Message = "Interview Type updated successfully.",
                    Data = data
                };
            }

            // 🔹 Success
            _logger.LogInformation("Interview Type created successfully. Interview_Type={Interview_Type}", model.Interview_Type);

            return new ApiResponse<InterviewTypeCreateUpdateResponseViewModel>
            {
                Success = true,
                Message = "Interview Type created successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<InterviewTypeListResponseViewModel>> DeleteInterviewTypeAsync(InterviewTypeDeleteRequestViewModel model)
        {
            // 🔹 Repository Call
            _logger.LogInformation("InterviewTypeService: DeleteInterviewTypeAsync START. with Id={Id}", model.Id);

            await _interviewTypeRepository.DeleteInterviewTypeAsync(model);

            // 🔹 Fetch Updated List
            var data = await _interviewTypeRepository.GetInterviewTypesAsync(
                Search: null,
                Length: 10,
                Page: 1,
                OrderColumn: "interview_type",
                OrderDirection: "Asc"
            );

            // 🔹 Success
            _logger.LogInformation("Interview Type deleted successfully. Id={Id}", model.Id);

            return new ApiResponse<InterviewTypeListResponseViewModel>
            {
                Success = true,
                Message = "Interview Type deleted successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<JobObjectiveResponse>> GetJobObjectiveAsync(string jobType, string workMode, int experienceYears, string objective)
        {
            _logger.LogInformation(
                "Service Start: GetJobObjectiveAsync | JobType={JobType}, WorkMode={WorkMode}, ExperienceYears={ExperienceYears}, objective={objective}",
                jobType, workMode, experienceYears, objective
            );

            try
            {
                // 🔹 Get Organization Id from Claims
                var organizationIdClaim = _contextAccessor
                    .HttpContext?
                    .User?
                    .FindFirst("organization_id")?
                    .Value;

                if (!Guid.TryParse(organizationIdClaim, out var organizationId))
                {
                    _logger.LogWarning(
                        "OrganizationId missing or invalid in claims | ClaimValue={ClaimValue}",
                        organizationIdClaim
                    );

                    return new ApiResponse<JobObjectiveResponse>
                    {
                        Success = false,
                        Message = "Invalid organization context.",
                    };
                }

                // 🔹 Get OpenAI Key (do NOT log the key)
                _logger.LogInformation(
                    "Fetching OpenAI key | OrganizationId={OrganizationId}",
                    organizationId
                );

                var openAIKey = await _interviewerRepository.GetOpenAiKey(organizationId);

                if (string.IsNullOrWhiteSpace(openAIKey))
                {
                    _logger.LogWarning(
                        "OpenAI key not found | OrganizationId={OrganizationId}",
                        organizationId
                    );

                    return new ApiResponse<JobObjectiveResponse>
                    {
                        Success = false,
                        Message = "OpenAI configuration missing."
                    };
                }

                // 🔹 Call AI Service
                _logger.LogInformation(
                    "Calling GeneralAIService.GetJobObjectiveAsync | OrganizationId={OrganizationId}",
                    organizationId
                );

                var jobObjective = await _iGeneralAIService
                    .GetJobObjectivesAsync(openAIKey, jobType, workMode, experienceYears, objective);

                if (string.IsNullOrWhiteSpace(jobObjective.JobObjective))
                {
                    _logger.LogWarning(
                        "AI returned empty objective | OrganizationId={OrganizationId}",
                        organizationId
                    );

                    return new ApiResponse<JobObjectiveResponse>
                    {
                        Success = false,
                        Message = "Unable to generate job objective."
                    };
                }

                // 🔹 Success
                _logger.LogInformation(
                    "Service Success: Job objective generated | OrganizationId={OrganizationId}",
                    organizationId
                );

                return new ApiResponse<JobObjectiveResponse>
                {
                    Success = true,
                    Message = "Interview objective retrieved successfully.",
                    Data = jobObjective
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception in GetJobObjectiveAsync | JobType={JobType}, WorkMode={WorkMode}, ExperienceYears={ExperienceYears}",
                    jobType, workMode, experienceYears
                );

                return new ApiResponse<JobObjectiveResponse>
                {
                    Success = false,
                    Message = "An unexpected error occurred while generating the interview objective."
                };
            }
        }


    }
}
