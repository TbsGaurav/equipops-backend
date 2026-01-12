using SettingService.Api.ViewModels.Request.EmailTemplate;
using SettingService.Api.ViewModels.Response.EmailTemplate;

namespace SettingService.Api.Infrastructure.Interface
{
    public interface IEmailTemplateRepository
    {
        Task<EmailTemplateDeleteResponseViewModel> EmailTemplateDeleteAsync(EmailTemplateDeleteRequestViewModel request);
        Task<EmailTemplateCreateUpdateResponseViewModel> CreateUpdateEmailTemplateAsync(EmailTemplateCreateUpdateRequestViewModel request);
        Task<EmailTemplateListResponseViewModel> EmailTemplateListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection);

        Task<EmailTemplateResponseViewModel> EmailTemplateByIdAsync(Guid id);
    }
}
