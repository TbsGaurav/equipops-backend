using SettingService.Api.ViewModels.Response.Master_Dropdown;

namespace SettingService.Api.Infrastructure.Interface
{
    public interface IMasterDropdownRepository
    {
        Task<MasterDropdownListResponseViewModel> GetMasterDropdownsAsync();
    }
}
