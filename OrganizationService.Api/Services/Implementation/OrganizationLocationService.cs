using Microsoft.AspNetCore.Mvc;

using OrganizationService.Api.Helpers.ResponseHelpers.Enums;
using OrganizationService.Api.Helpers.ResponseHelpers.Handlers;
using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.Services.Interface;
using OrganizationService.Api.ViewModels.Response.OrganizationLocation;

namespace OrganizationService.Api.Services.Implementation
{
    public class OrganizationLocationService : IOrganizationLocationService
    {
        private readonly IOrganizationLocationRepository _organizationLocationRepository;
        private readonly ILogger<OrganizationLocationService> _logger;

        public OrganizationLocationService(IOrganizationLocationRepository organizationLocationRepository, ILogger<OrganizationLocationService> logger)
        {
            _organizationLocationRepository = organizationLocationRepository;
            _logger = logger;
        }

        public async Task<IActionResult> GetCountryListAsync()
        {
            try
            {
                // 🔹 Repository Call
                _logger.LogInformation("Calling OrganizationRepository.GetCountryListAsync.");

                var data = await _organizationLocationRepository.GetCountriesAsync();

                // 🔹 Failure
                if (data == null)
                {
                    return new NotFoundObjectResult(ResponseHelper<string>.Error(
                        "Countries not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    ));
                }

                // 🔹 Success
                _logger.LogInformation("Countries retrieved successfully.");

                return new OkObjectResult(ResponseHelper<CountryListResponseViewModel>.Success(
                    "Countries retrieved successfully.",
                    data
                ));
            }
            catch (Exception ex)
            {
                // 🔹 Log Error
                _logger.LogError(ex, "Error retrieving country list.");

                return new ObjectResult(ResponseHelper<string>.Error(
                    "An internal server error occurred.",
                    exception: ex,
                    statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                ));
            }
        }

        public async Task<IActionResult> GetStateByCountryListAsync(Guid countryId)
        {
            try
            {
                // 🔹 Repository Call
                _logger.LogInformation("Calling OrganizationRepository.GetStateByCountryListAsync.");

                var data = await _organizationLocationRepository.GetStatesByCountryAsync(countryId);

                // 🔹 Failure
                if (data == null)
                {
                    return new NotFoundObjectResult(ResponseHelper<string>.Error(
                        "States not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    ));
                }

                // 🔹 Success
                _logger.LogInformation("States retrieved successfully.");

                return new OkObjectResult(ResponseHelper<StateByCountryListResponseViewModel>.Success(
                    "States retrieved successfully.",
                    data
                ));
            }
            catch (Exception ex)
            {
                // 🔹 Log Error
                _logger.LogError(ex, "Error retrieving state list.");

                return new ObjectResult(ResponseHelper<string>.Error(
                    "An internal server error occurred.",
                    exception: ex,
                    statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                ));
            }
        }

        public async Task<IActionResult> GetCityByStateListAsync(Guid stateId)
        {
            try
            {
                // 🔹 Repository Call
                _logger.LogInformation("Calling OrganizationRepository.GetCityByStateListAsync.");

                var data = await _organizationLocationRepository.GetCitiesByStateAsync(stateId);

                // 🔹 Failure
                if (data == null)
                {
                    return new NotFoundObjectResult(ResponseHelper<string>.Error(
                        "Cities not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    ));
                }

                // 🔹 Success
                _logger.LogInformation("Cities retrieved successfully.");

                return new OkObjectResult(ResponseHelper<CityByStateListResponseViewModel>.Success(
                    "Cities retrieved successfully.",
                    data
                ));
            }
            catch (Exception ex)
            {
                // 🔹 Log Error
                _logger.LogError(ex, "Error retrieving city list.");

                return new ObjectResult(ResponseHelper<string>.Error(
                    "An internal server error occurred.",
                    exception: ex,
                    statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                ));
            }
        }
    }
}
