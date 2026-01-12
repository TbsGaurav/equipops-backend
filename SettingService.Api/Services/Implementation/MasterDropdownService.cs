using Microsoft.AspNetCore.Mvc;

using SettingService.Api.Helpers.ResponseHelpers.Enums;
using SettingService.Api.Helpers.ResponseHelpers.Handlers;
using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.Services.Interface;
using SettingService.Api.ViewModels.Response.Master_Dropdown;

namespace SettingService.Api.Services.Implementation
{
    public class MasterDropdownService : IMasterDropdownService
    {
        private readonly IMasterDropdownRepository _masterDropdownRepository;
        private readonly ILogger<MasterDropdownService> _logger;

        public MasterDropdownService(
            IMasterDropdownRepository masterDropdownRepository,
            ILogger<MasterDropdownService> logger
            )
        {
            _masterDropdownRepository = masterDropdownRepository;
            _logger = logger;
        }

        public async Task<IActionResult> GetMasterDropdownsAsync()
        {
            try
            {
                // 🔹 Repository Call
                _logger.LogInformation("Calling MasterDropdownRepository.GetMasterDropdownsAsync.");

                var data = await _masterDropdownRepository.GetMasterDropdownsAsync();

                // 🔹 Failure
                if (data == null)
                {
                    return new NotFoundObjectResult(ResponseHelper<string>.Error(
                        "Master Dropdowns not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    ));
                }

                // 🔹 Success
                _logger.LogInformation("Master Dropdowns retrieved successfully.");

                return new OkObjectResult(ResponseHelper<MasterDropdownListResponseViewModel>.Success(
                    "Master Dropdowns retrieved successfully.",
                    data
                ));
            }
            catch (Exception ex)
            {
                // 🔹 Log Error
                _logger.LogError(ex, "Error retrieving master dropdown list.");

                return new ObjectResult(ResponseHelper<string>.Error(
                    "An internal server error occurred.",
                    exception: ex,
                    statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                ));
            }
        }
    }
}
