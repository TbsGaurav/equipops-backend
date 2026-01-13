using CommonHelper.Enums;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.EquipmentCategory;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EquipOps.Services.Implementation
{
    public sealed class EquipmentCategoryService : IEquipmentCategoryService
    {
        private readonly IPgHelper _pgHelper;
        private readonly ILogger<EquipmentCategoryService> _logger;

        public EquipmentCategoryService(IPgHelper pgHelper, ILogger<EquipmentCategoryService> logger)
        {
            _pgHelper = pgHelper;
            _logger = logger;
        }

        /* -------------------------------------------------------------
         * CREATE / UPDATE
         * -------------------------------------------------------------*/
        public async Task<IActionResult> EquipmentCategoryCreateAsync(EquipmentCategoryRequest request)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_return_category_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                    { "p_return_updated_at", new DbParam { DbType = DbType.DateTime, Direction = ParameterDirection.InputOutput } },
                    { "p_category_id", new DbParam { Value = request.category_id, DbType = DbType.Int32 } },
                    { "p_organization_id", new DbParam { Value = request.organization_id, DbType = DbType.Int32 } },
                    { "p_category_name", new DbParam { Value = request.category_name, DbType = DbType.String } },
                    { "p_description", new DbParam { Value = request.description, DbType = DbType.String } }
                };

                var result = await _pgHelper.CreateUpdateAsync(
                    "master.sp_equipment_category_create_update",
                    param
                );

                string message = request.category_id == null || request.category_id == 0
                    ? "Equipment category created successfully."
                    : "Equipment category updated successfully.";

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success(message, result)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Equipment Category save error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Internal server error.",
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        /* -------------------------------------------------------------
         * GET BY ID
         * -------------------------------------------------------------*/
        public async Task<IActionResult> EquipmentCategoryByIdAsync(int category_id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_category_id", new DbParam { Value = category_id, DbType = DbType.Int32 } },
                    { "ref", new DbParam { Value = "equipment_category_by_id_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
                };

                dynamic result = await _pgHelper.ListAsync(
                    "master.sp_equipment_category_getbyid",
                    param
                );

                var list = result.@ref as List<dynamic>;

                if (list == null || !list.Any())
                    return new NotFoundObjectResult(
                        ResponseHelper<string>.Error(
                            "Equipment category not found.",
                            statusCode: StatusCodeEnum.NOT_FOUND
                        )
                    );

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Equipment category found.", list.First())
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get Equipment Category error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Internal server error.",
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        /* -------------------------------------------------------------
         * DELETE
         * -------------------------------------------------------------*/
        public async Task<IActionResult> EquipmentCategoryDeleteAsync(int category_id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_return_category_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                    { "p_return_updated_at", new DbParam { DbType = DbType.DateTime, Direction = ParameterDirection.InputOutput } },
                    { "p_category_id", new DbParam { Value = category_id, DbType = DbType.Int32 } }
                };

                var result = await _pgHelper.CreateUpdateAsync(
                    "master.sp_equipment_category_delete",
                    param
                );

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Equipment category deleted successfully.", result)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Equipment Category error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Internal server error.",
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        /* -------------------------------------------------------------
         * LIST
         * -------------------------------------------------------------*/
        public async Task<IActionResult> EquipmentCategoryListAsync(
            string? search,
            int length,
            int page,
            string orderColumn,
            string orderDirection)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_search", new DbParam { Value = search, DbType = DbType.String } },
                    { "p_length", new DbParam { Value = length, DbType = DbType.Int32 } },
                    { "p_page", new DbParam { Value = page, DbType = DbType.Int32 } },
                    { "p_order_column", new DbParam { Value = orderColumn, DbType = DbType.String } },
                    { "p_order_direction", new DbParam { Value = orderDirection, DbType = DbType.String } },
                    { "o_total_records", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                    { "ref", new DbParam { Value = "equipment_category_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
                };

                dynamic result = await _pgHelper.ListAsync(
                    "master.sp_equipment_category_list",
                    param
                );

                var list = result.@ref as List<dynamic>;

                if (list == null || !list.Any())
                    return new NotFoundObjectResult(
                        ResponseHelper<string>.Error(
                            "No equipment categories found.",
                            statusCode: StatusCodeEnum.NOT_FOUND
                        )
                    );

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Equipment categories retrieved.", new
                    {
                        TotalNumbers = result.o_total_records,
                        CategoryData = list
                    })
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "List Equipment Category error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Internal server error.",
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }
    }
}
