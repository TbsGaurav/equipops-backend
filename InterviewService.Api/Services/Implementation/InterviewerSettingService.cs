using InterviewService.Api.Helpers.ResponseHelpers.Models;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interviewer_setting;
using InterviewService.Api.ViewModels.Response.Interviewer_setting;

namespace InterviewService.Api.Services.Implementation
{
    public class InterviewerSettingService(ILogger<InterviewerSettingService> _logger, IInterviewerSettingRepository interviewerSettingRepository) : IInterviewerSettingService
    {
        public async Task<ApiResponse<InterviewerSettingCreateUpdateResponseViewModel>> CreateUpdateInterviewerSettingAsync(InterviewerSettingCreateUpdateRequestViewModel model)
        {
            _logger.LogInformation("InterviewerSettingService: CreateUpdateInterviewerSettingAsync START. Name={Name}", model.Name);

            // 🔹 Validate Input
            if (string.IsNullOrEmpty(model.Name))
            {
                _logger.LogWarning("Validation failed: Required fields are missing. Name={Name}", model.Name);

                return new ApiResponse<InterviewerSettingCreateUpdateResponseViewModel>
                {
                    Success = false,
                    Message = "Interviewer First name is required."
                };
            }
            // 🔹 Repository Call
            _logger.LogInformation("Calling InterviewerSettingRepository.CreateUpdateInterviewerSettingAsync for Name={Name}", model.Name);

            var data = await interviewerSettingRepository.CreateUpdateInterviewerSettingAsync(model);

            if (data == null || data.Id == Guid.Empty)
            {
                _logger.LogWarning("Interviewer setting creation/updation failed. No Id returned. Name={Name}", model.Name);

                return new ApiResponse<InterviewerSettingCreateUpdateResponseViewModel>
                {
                    Success = false,
                    Message = "Interviewer setting creation/updation failed.",
                    Data = data
                };
            }

            if (model.Id != null)
            {
                _logger.LogInformation("Interviewer setting updated successfully. Name={Name}", model.Name);

                return new ApiResponse<InterviewerSettingCreateUpdateResponseViewModel>
                {
                    Success = true,
                    Message = "Interviewer setting updated successfully.",
                    Data = data
                };
            }

            // 🔹 Success
            _logger.LogInformation("Interviewer setting created successfully. Name={Name}", model.Name);

            return new ApiResponse<InterviewerSettingCreateUpdateResponseViewModel>
            {
                Success = true,
                Message = "Interviewer setting created successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<InterviewerSettingListResponseViewModel>> DeleteInterviewerSettingAsync(InterviewerSettingDeleteRequestViewModel model)
        {

            // 🔹 Repository Call
            _logger.LogInformation("InterviewerSettingService: DeleteInterviewerSettingAsync START. with Id={Id}", model.Id);

            await interviewerSettingRepository.DeleteInterviewerSettingAsync(model);

            // 🔹 Fetch Updated List
            var data = await interviewerSettingRepository.GetInterviewerSettingsAsync(
                Search: null,
                Length: 10,
                Page: 1,
                OrderColumn: "first_name",
                OrderDirection: "Asc"
            );

            // 🔹 Success
            _logger.LogInformation("Interviewer setting deleted successfully. Id={Id}", model.Id);

            return new ApiResponse<InterviewerSettingListResponseViewModel>
            {
                Success = true,
                Message = "Interviewer setting deleted successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<InterviewerSettingData>> GetInterviewerSettingByIdAsync(Guid Id)
        {

            // 🔹 Repository Call
            _logger.LogInformation("Calling InterviewerSettingRepository.GetInterviewerSettingByIdAsync.");

            var data = await interviewerSettingRepository.GetInterviewerSettingByIdAsync(Id);

            // 🔹 Success
            _logger.LogInformation("Interviewer setting retrieved successfully.");

            return new ApiResponse<InterviewerSettingData>
            {
                Success = true,
                Message = "Interviewer setting retrieved successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<InterviewerSettingListResponseViewModel>> GetInterviewerSettingsAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null)
        {

            // 🔹 Repository Call
            _logger.LogInformation("Calling InterviewerSettingRepository.GetInterviewerSettingAsync.");

            var data = await interviewerSettingRepository.GetInterviewerSettingsAsync(Search, Length, Page, OrderColumn, OrderDirection, IsActive);

            // 🔹 Success
            _logger.LogInformation("Interviewer setting retrieved successfully.");

            return new ApiResponse<InterviewerSettingListResponseViewModel>
            {
                Success = true,
                Message = "Interviewers setting retrieved successfully.",
                Data = data
            };
        }
    }
}
