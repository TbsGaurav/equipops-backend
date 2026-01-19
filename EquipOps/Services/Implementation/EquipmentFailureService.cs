using CommonHelper.constants;
using CommonHelper.Enums;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.EquipmentFailure;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EquipOps.Services.Implementation
{
    public class EquipmentFailureService(IPgHelper pgHelper, ILogger<EquipmentFailureService> logger) : IEquipmentFailureService
    {
        public async Task<IActionResult> EquipmentFailureCreateAsync(EquipmentFailureRequest request)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_return_failure_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                    { "p_return_updated_at", new DbParam { DbType = DbType.DateTime, Direction = ParameterDirection.InputOutput } },
                    { "p_failure_id", new DbParam { Value = request.failure_id, DbType = DbType.Int32 } },
                    { "p_organization_id", new DbParam { Value = request.organization_id, DbType = DbType.Int32 } },
                    { "p_equipment_id", new DbParam { Value = request.equipment_id, DbType = DbType.Int32 } },
                    { "p_subpart_id", new DbParam { Value = request.subpart_id, DbType = DbType.Int32 } },
                    { "p_failure_date", new DbParam { Value = request.failure_date, DbType = DbType.DateTime } },
                    { "p_failure_type", new DbParam { Value = request.failure_type, DbType = DbType.String } },
                    { "p_description", new DbParam { Value = request.description, DbType = DbType.String } },
                    { "p_downtime_minutes", new DbParam { Value = request.downtime_minutes, DbType = DbType.Int32 } }
                };

                var result = await pgHelper.CreateUpdateAsync("master.sp_equipment_failure_create_update",param);

                string message = request.failure_id == null || request.failure_id == 0
                    ? "Equipment failure created successfully."
                    : "Equipment failure updated successfully.";

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success(message, result)
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Equipment Failure save error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        ConstantMessages.InternalServerErrorMessage,
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        public async Task<IActionResult> EquipmentFailureByIdAsync(int failure_id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_failure_id", new DbParam { Value = failure_id, DbType = DbType.Int32 } },
                    { "ref", new DbParam { Value = "equipment_failure_by_id_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
                };

                dynamic result = await pgHelper.ListAsync("master.sp_equipment_failure_getbyid",param);

                var list = result.@ref as List<dynamic>;

                if (list == null || !list.Any())
                    return new NotFoundObjectResult(
                        ResponseHelper<string>.Error(
                            "Equipment failure not found.",
                            statusCode: StatusCodeEnum.NOT_FOUND
                        )
                    );

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Equipment failure found.", list[0])
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get Equipment Failure error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        ConstantMessages.InternalServerErrorMessage,
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        public async Task<IActionResult> EquipmentFailureDeleteAsync(int failure_id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_return_failure_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                    { "p_failure_id", new DbParam { Value = failure_id, DbType = DbType.Int32 } }
                };

                var result = await pgHelper.CreateUpdateAsync("master.sp_equipment_failure_delete",param);

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Equipment failure deleted successfully.", result)
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Delete Equipment Failure error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        ConstantMessages.InternalServerErrorMessage,
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        public async Task<IActionResult> EquipmentFailureListAsync(string? search,int length,int page,string orderColumn,string orderDirection)
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
                    { "ref", new DbParam { Value = "equipment_failure_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
                };

                dynamic result = await pgHelper.ListAsync("master.sp_equipment_failure_list",param);

                var list = result.@ref as List<dynamic>;

                if (list == null || !list.Any())
                    return new NotFoundObjectResult(
                        ResponseHelper<string>.Error(
                            "No equipment failures found.",
                            statusCode: StatusCodeEnum.NOT_FOUND
                        )
                    );

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Equipment failures retrieved.", new
                    {
                        TotalNumbers = result.o_total_records,
                        FailureData = list
                    })
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "List Equipment Failure error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        ConstantMessages.InternalServerErrorMessage,
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }
    }
}
