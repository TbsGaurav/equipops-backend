using Microsoft.AspNetCore.Mvc;

namespace OrganizationService.Api.Services.Interface
{
    public interface IOrganizationLocationService
    {
        Task<IActionResult> GetCountryListAsync();
        Task<IActionResult> GetStateByCountryListAsync(Guid countryId);
        Task<IActionResult> GetCityByStateListAsync(Guid stateId);
    }
}
