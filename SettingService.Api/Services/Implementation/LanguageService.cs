using Microsoft.AspNetCore.Mvc;

using SettingService.Api.Helpers.ResponseHelpers.Enums;
using SettingService.Api.Helpers.ResponseHelpers.Handlers;
using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.Services.Interface;
using SettingService.Api.ViewModels.Request.Language;
using SettingService.Api.ViewModels.Response.Language;

namespace SettingService.Api.Services.Implementation
{
    public class LanguageService(ILogger<LanguageService> _logger, ILanguageRepository languageRepository) : ILanguageService
    {
        public async Task<IActionResult> CreateUpdateLanguageAsync(LanguageCreateUpdateRequestViewModel model)
        {
            _logger.LogInformation(
                "LanguageService: CreateUpdate START. Code={Code}",
                model?.Code
            );

            // 🔹 Validation
            if (model == null ||
                string.IsNullOrWhiteSpace(model.Code) ||
                string.IsNullOrWhiteSpace(model.Name))
            {
                _logger.LogWarning(
                    "Validation failed: Required fields missing. Name={Name}, Code={Code}",
                    model?.Name,
                    model?.Code
                );

                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Name and Code are required.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            if (model.Direction != "ltr" && model.Direction != "rtl")
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error("Direction must be ltr or rtl")
                );
            }

            _logger.LogInformation(
                "Calling LanguageRepository.CreateUpdateLanguageAsync for Code={Code}",
                model.Code
            );

            var data = await languageRepository.CreateUpdateLanguageAsync(model);

            if (data == null || data.Id == Guid.Empty)
            {
                _logger.LogWarning(
                    "Create/Update failed. No language returned. Code={Code}",
                    model.Code
                );

                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Language create/update failed.",
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }

            _logger.LogInformation(
                "Language create/update successful. LanguageId={LanguageId}, Code={Code}",
                data.Id,
                data.Code
            );

            return new OkObjectResult(
                ResponseHelper<LanguageCreateUpdateResponseViewModel>.Success(
                    model.Id == Guid.Empty
                        ? "Language created successfully."
                        : "Language updated successfully.",
                    data
                )
            );
        }
        public async Task<IActionResult> DeleteLanguageAsync(LanguageDeleteRequestViewModel request)
        {
            _logger.LogInformation(
                "LanguageService: Deleting language with LanguageId={LanguageId}",
                request?.Id
            );

            if (request == null || request.Id == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Invalid language ID.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            await languageRepository.DeleteLanguageAsync(request);

            // 🔹 Fetch updated list after deletion
            var updatedList = await languageRepository.GetLanguageListAsync(
                Search: null,
                Length: 10,
                Page: 1,
                OrderColumn: "name",
                OrderDirection: "Asc"
            );

            return new OkObjectResult(
                ResponseHelper<LanguageListResponseViewModel>.Success(
                    "Language deleted successfully.",
                    updatedList
                )
            );
        }
        public async Task<IActionResult> GetLanguageByIdAsync(Guid? id)
        {
            _logger.LogInformation(
                "LanguageService: Fetching language. Id={Id}",
                id
            );

            if (id == null || id == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Invalid language ID.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            var data = await languageRepository.GetLanguageByIdAsync(id);

            if (data == null)
            {
                return new NotFoundObjectResult(
                    ResponseHelper<string>.Error(
                        "Language not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    )
                );
            }

            return new OkObjectResult(
                ResponseHelper<LanguageData>.Success(
                    "Language fetched successfully.",
                    data
                )
            );
        }
        public async Task<IActionResult> GetLanguageListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null)
        {
            _logger.LogInformation(
                "LanguageService: Fetching language list. Search={Search}, Page={Page}, Length={Length}",
                Search, Page, Length);

            var data = await languageRepository.GetLanguageListAsync(Search, Length, Page, OrderColumn, OrderDirection, isActive);

            return new OkObjectResult(
                  ResponseHelper<LanguageListResponseViewModel>.Success(
                      "Language list fetched successfully.",
                      data
                  )
            );
        }
    }
}
