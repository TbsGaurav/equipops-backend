using OrganizationService.Api.ViewModels.Request.OrganzationSetting;
using OrganizationService.Api.ViewModels.Response.OrganizationSetting;

namespace OrganizationService.Api.Infrastructure.Interface
{
    public interface IOrganizationSettingRepository
    {
        Task<List<OrganizationSettingListResponseViewModel>> GetOrganizationSettingsAsync();
        Task<OrganizationSettingByKeyResponseViewModel> GetOrganizationSettingByKeyAsync(string Key);
        Task<OrganizationSettingInternalResult> CreateUpdateOrganizationSettingAsync(OrganizationSettingCreateUpdateRequestViewModel request);
        Task<string?> GetRetellAiKey(Guid organizationId);
    }
}
