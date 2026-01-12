using Common.Services.Helper;

using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.Services.Interface;
using SettingService.Api.ViewModels.Request.EmailTemplate;
using SettingService.Api.ViewModels.Response.EmailTemplate;

namespace SettingService.Api.Services.Implementation
{
    public class EmailTemplateService(ILogger<EmailTemplateService> _logger, IEmailTemplateRepository emailTemplateRepository) : IEmailTemplateService
    {
        public async Task<ApiResponse<EmailTemplateDeleteResponseViewModel>> EmailTemplateDeleteAsync(EmailTemplateDeleteRequestViewModel model)
        {
            //🔹 Repository Call

            var data = await emailTemplateRepository.EmailTemplateDeleteAsync(model);

            string Message = "";
            bool Status = false;

            if (data.id == null)
                Message = "Invalid data";
            else
            {
                Status = true;
                Message = "User role is deleted successfully.";
            }

            return new ApiResponse<EmailTemplateDeleteResponseViewModel>
            {
                StatusCode = 1,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<EmailTemplateCreateUpdateResponseViewModel>> CreateUpdateEmailTemplateAsync(EmailTemplateCreateUpdateRequestViewModel model)
        {
            var data = await emailTemplateRepository.CreateUpdateEmailTemplateAsync(model);
            if (data == null || data.id == Guid.Empty)
            {
                return new ApiResponse<EmailTemplateCreateUpdateResponseViewModel>
                {
                    Success = false,
                    Message = "Subscription type create/update failed.",
                    Data = data
                };
            }
            return new ApiResponse<EmailTemplateCreateUpdateResponseViewModel>
            {
                StatusCode = 200,
                Success = true,
                Message = model.id == null
                    ? "Subscription type created successfully."
                    : "Subscription type updated successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<EmailTemplateListResponseViewModel>> EmailTemplateListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection)
        {
            string Message = "";
            bool Status = false;

            //🔹 Repository Call
            var data = await emailTemplateRepository.EmailTemplateListAsync(search, IsActive, length, page, orderColumn, orderDirection);

            if (data == null)
                Message = "Invalid data.";
            else
            {
                Status = true;
                Message = "Success.";
            }

            return new ApiResponse<EmailTemplateListResponseViewModel>
            {
                StatusCode = 200,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<EmailTemplateResponseViewModel>> EmailTemplateByIdAsync(Guid id)
        {
            _logger.LogInformation("SubscriptionService: Fetching subscription list. Search={id}", id);

            var data = await emailTemplateRepository.EmailTemplateByIdAsync(id);

            return new ApiResponse<EmailTemplateResponseViewModel>
            {
                Success = true,
                Message = "Subscription ById fetched successfully.",
                Data = data
            };
        }
    }
}
