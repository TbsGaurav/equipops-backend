using Common.Services.Helper;

using Microsoft.AspNetCore.Mvc;

using SettingService.Api.Helpers.ResponseHelpers.Enums;
using SettingService.Api.Helpers.ResponseHelpers.Handlers;
using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.Services.Interface;
using SettingService.Api.ViewModels.Request.Menu_type;
using SettingService.Api.ViewModels.Response.Menu_type;

namespace SettingService.Api.Services.Implementation
{
    public class MenuTypeService(ILogger<MenuTypeService> _logger, IMenuTypeRepository menuTypeRepository) : IMenuTypeService
    {
        #region Create / Update Menu Type
        public async Task<IActionResult> CreateUpdateMenuTypeAsync(MenuTypeCreateUpdateRequestViewModel model)
        {
            _logger.LogInformation("MenuTypeService: CreateUpdate START. Name={Name}", model?.Name);

            if (model == null || string.IsNullOrWhiteSpace(model.Name))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Name is required.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            var data = await menuTypeRepository.CreateUpdateMenuTypeAsync(model);

            if (data == null || data.Id == Guid.Empty)
            {
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Menu type create/update failed.",
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }

            return new OkObjectResult(
                ResponseHelper<MenuTypeCreateUpdateResponseViewModel>.Success(
                    model.Id == Guid.Empty
                        ? "Menu type created successfully."
                        : "Menu type updated successfully.",
                    data
                )
            );
        }
        #endregion

        #region Delete Menu Type
        public async Task<IActionResult> DeleteMenuTypeAsync(MenuTypeDeleteRequestViewModel request)
        {
            _logger.LogInformation("MenuTypeService: Deleting menu type with MenuTypeId={MenuTypeId}", request?.Id);

            if (request == null || request.Id == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Invalid menu type ID.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            await menuTypeRepository.DeleteMenuTypeAsync(request);

            var updatedList = await menuTypeRepository.GetMenuTypeListAsync(
                Search: null,
                Length: 10,
                Page: 1,
                OrderColumn: "name",
                OrderDirection: "Asc"
            );

            return new OkObjectResult(
                ResponseHelper<MenuTypeListResponseViewModel>.Success(
                    "Menu type deleted successfully.",
                    updatedList
                )
            );
        }
        #endregion

        #region Get Menu Type By Id
        public async Task<IActionResult> GetMenuTypeByIdAsync(Guid? id)
        {
            _logger.LogInformation("MenuTypeService: Fetching menu type. Id={Id}", id);

            if (id == null || id == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Invalid menu type ID.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            var data = await menuTypeRepository.GetMenuTypeByIdAsync(id);

            if (data == null)
            {
                return new NotFoundObjectResult(
                    ResponseHelper<string>.Error(
                        "Menu type not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    )
                );
            }

            return new OkObjectResult(
                ResponseHelper<MenuTypeData>.Success(
                    "Menu type fetched successfully.",
                    data
                )
            );
        }
        #endregion

        #region Get Menu Type List
        public async Task<IActionResult> GetMenuTypeListAsync(string? search, int length, int page, string orderColumn, string orderDirection = "Asc", bool? isActive = null)
        {
            _logger.LogInformation("MenuTypeService: Fetching menu type list. Search={Search}, Page={Page}, Length={Length}", search, page, length);

            var data = await menuTypeRepository.GetMenuTypeListAsync(
                search,
                length,
                page,
                orderColumn,
                orderDirection,
                isActive
            );

            return new OkObjectResult(
                ResponseHelper<MenuTypeListResponseViewModel>.Success(
                    "Menu type list fetched successfully.",
                    data
                )
            );
        }
        #endregion

        #region Menu Permission
        public async Task<ApiResponse<MenuPermissionDeleteResponseViewModel>> MenuPermissionDeleteAsync(MenuPermissionDeleteRequestViewModel model)
        {
            //🔹 Repository Call

            var data = await menuTypeRepository.MenuPermissionDeleteAsync(model);

            string Message = "";
            bool Status = false;

            if (data.id == null)
                Message = "Invalid data";
            else
            {
                Status = true;
                Message = "Menu Permission is deleted successfully.";
            }

            return new ApiResponse<MenuPermissionDeleteResponseViewModel>
            {
                StatusCode = 1,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<MenuPermissionCreateUpdateResponseViewModel>> CreateUpdateMenuPermissionAsync(MenuPermissionCreateUpdateRequestViewModel model)
        {
            var data = await menuTypeRepository.CreateUpdateMenuPermissionAsync(model);
            if (data == null || data.id == Guid.Empty)
            {
                return new ApiResponse<MenuPermissionCreateUpdateResponseViewModel>
                {
                    Success = false,
                    Message = "Menu Permission create/update failed.",
                    Data = data
                };
            }
            return new ApiResponse<MenuPermissionCreateUpdateResponseViewModel>
            {
                StatusCode = 200,
                Success = true,
                Message = model.id == null
                    ? "Menu Permission created successfully."
                    : "Menu Permission updated successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<MenuPermissionListResponseViewModel>> MenuPermissionListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection)
        {
            string Message = "";
            bool Status = false;

            //🔹 Repository Call
            var data = await menuTypeRepository.MenuPermissionListAsync(search, IsActive, length, page, orderColumn, orderDirection);

            if (data == null)
                Message = "Invalid data.";
            else
            {
                Status = true;
                Message = "Success.";
            }

            return new ApiResponse<MenuPermissionListResponseViewModel>
            {
                StatusCode = 200,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<MenuPermissionResponseViewModel>> MenuPermissionByIdAsync(Guid? id)
        {
            _logger.LogInformation("MenuTypeService: Fetching menu permission list. Search={id}", id);

            var data = await menuTypeRepository.MenuPermissionByIdAsync(id);

            return new ApiResponse<MenuPermissionResponseViewModel>
            {
                Success = true,
                Message = "Menu Permission ById fetched successfully.",
                Data = data
            };
        }
        #endregion
    }
}
