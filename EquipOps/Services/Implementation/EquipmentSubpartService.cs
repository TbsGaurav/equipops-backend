using CommonHelper.Enums;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.EquipmentSubpart;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EquipOps.Services.Implementation
{
    public class EquipmentSubpartService(IPgHelper pgHelper, ILogger<EquipmentSubpartService> logger) : IEquipmentSubpartService
    {
        public async Task<IActionResult> EquipmentSubpartCreateUpdateAsync(EquipmentSubpartRequest request)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_return_subpart_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                    { "p_return_updated_at", new DbParam { DbType = DbType.DateTime, Direction = ParameterDirection.InputOutput } },
                    { "p_subpart_id", new DbParam { Value = request.subpart_id, DbType = DbType.Int32 } },
                    { "p_equipment_id", new DbParam { Value = request.equipment_id, DbType = DbType.Int32 } },
                    { "p_subpart_name", new DbParam { Value = request.subpart_name, DbType = DbType.String } },
                    { "p_description", new DbParam { Value = request.description, DbType = DbType.String } },
                    { "p_status", new DbParam { Value = request.status, DbType = DbType.String } },
                    { "p_qr_code", new DbParam { Value = request.qr_code, DbType = DbType.String } }
                };

                var result = await pgHelper.CreateUpdateAsync(
                    "master.sp_equipment_subpart_create_update",
                    param
                );

                string message = request.subpart_id == null || request.subpart_id == 0
                    ? "Equipment subpart created successfully."
                    : "Equipment subpart updated successfully.";

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success(message, result)
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Equipment Subpart save error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Internal server error.",
                        exception: ex,
                        statusCode: CommonHelper.Enums.StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        public async Task<IActionResult> EquipmentSubpartByIdAsync(int subpart_id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_subpart_id", new DbParam { Value = subpart_id, DbType = DbType.Int32 } },
                    { "ref", new DbParam { Value = "equipment_subpart_by_id_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
                };

                dynamic result = await pgHelper.ListAsync(
                    "master.sp_equipment_subpart_getbyid",
                    param
                );

                var list = result.@ref as List<dynamic>;

                if (list == null || !list.Any())
                    return new NotFoundObjectResult(
                        ResponseHelper<string>.Error("Equipment subpart not found.", statusCode:CommonHelper.Enums.StatusCodeEnum.NOT_FOUND)
                    );

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Equipment subpart found.", list.First())
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get Equipment Subpart error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Internal server error.",
                        exception: ex,
                        statusCode: CommonHelper.Enums.StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        public async Task<IActionResult> EquipmentSubpartDeleteAsync(int subpart_id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_return_subpart_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                    { "p_subpart_id", new DbParam { Value = subpart_id, DbType = DbType.Int32 } }
                };

                var result = await pgHelper.CreateUpdateAsync(
                    "master.sp_equipment_subpart_delete",
                    param
                );

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Equipment subpart deleted successfully.", result)
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Delete Equipment Subpart error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Internal server error.",
                        exception: ex,
                        statusCode:CommonHelper.Enums.StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        public async Task<IActionResult> EquipmentSubpartListAsync(string? search,int length,int page,string orderColumn,string orderDirection)
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
                    { "ref", new DbParam { Value = "equipment_subpart_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
                };

                dynamic result = await pgHelper.ListAsync(
                    "master.sp_equipment_subpart_list",
                    param
                );

                var list = result.@ref as List<dynamic>;

                if (list == null || !list.Any())
                    return new NotFoundObjectResult(
                        ResponseHelper<string>.Error("No equipment subparts found.", statusCode:CommonHelper.Enums.StatusCodeEnum.NOT_FOUND)
                    );

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Equipment subparts retrieved.", new
                    {
                        TotalNumbers = result.o_total_records,
                        SubpartData = list
                    })
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "List Equipment Subpart error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Internal server error.",
                        exception: ex,
                        statusCode: CommonHelper.Enums.StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }
        public async Task<IActionResult> EquipmentSubpartDropdownAsync()
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    {
                        "ref",
                        new DbParam
                        {
                            Value = "subpart_cursor",
                            DbType = DbType.String,
                            Direction = ParameterDirection.InputOutput
                        }
                    }
                };

                dynamic result = await pgHelper.ListAsync("master.sp_equipment_subpart_dropdown", param);

                var list = result.@ref as List<dynamic>;

                return new OkObjectResult(ResponseHelper<dynamic>.Success("Subparts loaded.", list));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Equipment subpart dropdown error");
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