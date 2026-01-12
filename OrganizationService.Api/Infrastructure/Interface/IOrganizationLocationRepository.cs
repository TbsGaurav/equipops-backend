using OrganizationService.Api.ViewModels.Response.OrganizationLocation;

namespace OrganizationService.Api.Infrastructure.Interface
{
    public interface IOrganizationLocationRepository
    {
        Task<CountryListResponseViewModel> GetCountriesAsync();
        Task<StateByCountryListResponseViewModel> GetStatesByCountryAsync(Guid countryId);
        Task<CityByStateListResponseViewModel> GetCitiesByStateAsync(Guid stateId);
    }
}