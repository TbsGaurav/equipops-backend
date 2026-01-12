using Common.Services.Helper;

using OrganizationService.Api.ViewModels.Request.OrganzationSetting;
using OrganizationService.Api.ViewModels.Response.OrganizationSetting;

namespace OrganizationService.Api.Services.Interface
{
    public interface IOrganizationSettingService
    {
        Task<ApiResponse<List<OrganizationSettingListResponseViewModel>>> GetOrganizationSettingsAsync();
        Task<ApiResponse<OrganizationSettingByKeyResponseViewModel>> GetOrganizationSettingByKeyAsync(string Key);
        Task<ApiResponse<OrganizationSettingCreateUpdateResponseViewModel>> CreateUpdateOrganizationSettingAsync(OrganizationSettingCreateUpdateRequestViewModel model);
        Task<ApiResponse<OrganizationSettingCreateUpdateRequestViewModel>> Create_retell_llm_key();
    }
}
