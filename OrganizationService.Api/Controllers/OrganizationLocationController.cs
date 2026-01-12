using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OrganizationService.Api.Services.Interface;

namespace OrganizationService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationLocationController
    {
        private readonly IOrganizationLocationService _organizationLocationService;

        public OrganizationLocationController(IOrganizationLocationService organizationLocationService)
        {
            _organizationLocationService = organizationLocationService;
        }

        [AllowAnonymous]
        [HttpGet("get-county-list")]
        public async Task<IActionResult> GetCountries()
        {
            var result = await _organizationLocationService.GetCountryListAsync();
            return result;
        }

        [AllowAnonymous]
        [HttpGet("get-states-by-country")]
        public async Task<IActionResult> GetStatesByCountry(Guid countryId)
        {
            var result = await _organizationLocationService.GetStateByCountryListAsync(countryId);
            return result;
        }

        [AllowAnonymous]
        [HttpGet("get-cities-by_state")]
        public async Task<IActionResult> GetCitiesByState(Guid stateId)
        {
            var result = await _organizationLocationService.GetCityByStateListAsync(stateId);
            return result;
        }
    }
}
