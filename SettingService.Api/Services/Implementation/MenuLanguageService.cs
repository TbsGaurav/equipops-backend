using Microsoft.AspNetCore.Mvc;

using SettingService.Api.Helpers.ResponseHelpers.Enums;
using SettingService.Api.Helpers.ResponseHelpers.Handlers;
using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.Services.Interface;
using SettingService.Api.ViewModels.Request.MenuLanguage;
using SettingService.Api.ViewModels.Response.MenuLanguage;

namespace SettingService.Api.Services.Implementation
{
    public class MenuLanguageService(ILogger<MenuLanguageService> _logger, IMenuLanguageRepository menuLanguageRepository) : IMenuLanguageService
    {
        #region Create Update Menu Language
        public async Task<IActionResult> CreateUpdateMenuLanguageAsync(MenuLanguageCreateUpdateRequestViewModel model)
        {
            _logger.LogInformation("ModuleService: CreateUpdate START. Key_name={Key_name}", model?.Key_name);
            // 🔹 Validation
            if (model == null || string.IsNullOrWhiteSpace(model.Key_name))
            {
                _logger.LogWarning("Validation failed for required fields.");

                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Key name is required.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }
            var data = await menuLanguageRepository.CreateUpdateMenuLanguageAsync(model);
            if (data == null || data.Id == Guid.Empty)
            {
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Menu Language create/update failed.",
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
            return new OkObjectResult(
                ResponseHelper<MenuLanguageCreateUpdateResponseViewModel>.Success(
                    model.Id == Guid.Empty
                        ? "Menu Language created successfully."
                        : "Menu Language updated successfully.",
                    data
                )
            );
        }
        #endregion

        #region Delete Menu Language
        public async Task<IActionResult> DeleteMenuLanguageAsync(MenuLanguageDeleteRequestViewModel request)
        {
            _logger.LogInformation("MenuLanguageService: Deleting Menu Language with MenuLanguageId={MenuLanguageId}", request?.Id);

            if (request == null || request.Id == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Invalid Menu Language ID.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }
            // 🔹 Delete Menu Language
            await menuLanguageRepository.DeleteMenuLanguageAsync(request);

            // 🔹 Fetch updated list after deletion
            var updatedList = await menuLanguageRepository.GetMenuLanguageListAsync(
                Search: null,
                Length: 10,
                Page: 1,
                OrderColumn: "name",
                OrderDirection: "Asc"
            );

            return new OkObjectResult(
                ResponseHelper<MenuLanguageListResponseViewModel>.Success(
                    "Menu Language deleted successfully.",
                    updatedList
                )
            );
        }
        #endregion

        #region Get MenuLanguage By Id
        public async Task<IActionResult> GetMenuLanguageByIdAsync(Guid? id)
        {
            _logger.LogInformation("MenuLanguageService: Fetching Menu Language. Id={Id}", id);

            if (id == null || id == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Invalid Menu Language ID.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            var data = await menuLanguageRepository.GetMenuLanguageByIdAsync(id);

            if (data == null)
            {
                return new NotFoundObjectResult(
                    ResponseHelper<string>.Error(
                        "Menu Language not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    )
                );
            }

            return new OkObjectResult(
                ResponseHelper<MenuLanguageData>.Success(
                    "Menu Language fetched successfully.",
                    data
                )
            );
        }
        #endregion

        #region Get Menu Language List
        public async Task<IActionResult> GetMenuLanguageListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null)
        {
            _logger.LogInformation(
                "MenuLanguageService: Fetching Menu Language list. Search={Search}, Page={Page}, Length={Length}",
                Search, Page, Length);

            var data = await menuLanguageRepository.GetMenuLanguageListAsync(Search, Length, Page, OrderColumn, OrderDirection, isActive);

            return new OkObjectResult(
                  ResponseHelper<MenuLanguageListResponseViewModel>.Success(
                      "Menu Language list fetched successfully.",
                      data
                  )
              );
        }
        #endregion

        #region Get Menu Language By Language 
        public async Task<IActionResult> GetMenuLanguageByLanguageAsync(Guid? languageId)
        {
            _logger.LogInformation("MenuLanguageService: Fetching Menu Language. LanguageId={LanguageId}", languageId);

            if (languageId == null || languageId == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Invalid Language Id.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            var data = await menuLanguageRepository.GetMenuLanguageByLanguageAsync(languageId);

            if (data == null)
            {
                return new NotFoundObjectResult(
                    ResponseHelper<string>.Error(
                        "Menu Language not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    )
                );
            }

            return new OkObjectResult(
                ResponseHelper<Dictionary<string, string>>.Success(
                    "Menu Language fetched successfully.",
                    data
                )
            );
        }
        #endregion

        #region Update Menu Language By Language (Bulk)
        public async Task<IActionResult> MenuLanguageByLanguageUpdateAsync(MenuLanguageByLanguageUpdateRequestViewModel request)
        {
            _logger.LogInformation("MenuLanguageService: Updating Menu Language. LanguageId={LanguageId}", request.LanguageId);

            // 🔒 Validation
            if (request.LanguageId == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Invalid Language Id.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            if (request.Data == null || !request.Data.Any())
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "No Menu Language data provided.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            // 🚀 Call repository
            await menuLanguageRepository.MenuLanguageByLanguageUpdateAsync(request);

            return new OkObjectResult(
                ResponseHelper<string>.Success(
                    "Menu Language updated successfully."
                )
            );
        }
        #endregion
    }
}
