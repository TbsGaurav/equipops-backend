using Common.Services.Helper;

using SettingService.Api.ViewModels.Request.EmailTemplate;
using SettingService.Api.ViewModels.Response.EmailTemplate;

namespace SettingService.Api.Services.Interface
{
    public interface IEmailTemplateService
    {
        Task<ApiResponse<EmailTemplateDeleteResponseViewModel>> EmailTemplateDeleteAsync(EmailTemplateDeleteRequestViewModel model);
        Task<ApiResponse<EmailTemplateCreateUpdateResponseViewModel>> CreateUpdateEmailTemplateAsync(EmailTemplateCreateUpdateRequestViewModel model);
        Task<ApiResponse<EmailTemplateListResponseViewModel>> EmailTemplateListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection);

        Task<ApiResponse<EmailTemplateResponseViewModel>> EmailTemplateByIdAsync(Guid id);
    }
}
