using CommonHelper.Constants;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.DowntimeLog;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EquipOps.Services.Implementation
{
    public class DowntimeLogService(IPgHelper pgHelper, ILogger<DowntimeLogService> logger) : IDowntimeLogService
    {
        #region DowntimeLog Create Update

        public async Task<IActionResult> DowntimeLogCreateUpdateAsync(DowntimeLogRequest request)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_return_downtime_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                    { "p_return_updated_at", new DbParam { DbType = DbType.DateTime, Direction = ParameterDirection.InputOutput } },
                
                    { "p_downtime_id", new DbParam { Value = request.downtime_id ?? 0, DbType = DbType.Int32 } },
                    { "p_organization_id", new DbParam { Value = request.organization_id, DbType = DbType.Int32 } },
                    { "p_equipment_id", new DbParam { Value = request.equipment_id, DbType = DbType.Int32 } },
                    { "p_subpart_id", new DbParam { Value = request.subpart_id, DbType = DbType.Int32 } },
                    { "p_reported_by", new DbParam { Value = request.reported_by, DbType = DbType.Guid } },

                    {"p_start_time",new DbParam{Value = DateTime.SpecifyKind(request.start_time, DateTimeKind.Utc),DbType = DbType.DateTime}},
                    {"p_end_time",new DbParam{Value = request.end_time.HasValue? DateTime.SpecifyKind(request.end_time.Value, DateTimeKind.Utc): null,DbType = DbType.DateTime} },

                    { "p_reason", new DbParam { Value = request.reason, DbType = DbType.String } },
                    { "p_severity", new DbParam { Value = request.severity, DbType = DbType.String } },
                    { "p_cost", new DbParam { Value = request.cost, DbType = DbType.Decimal } }
                };

                var result = await pgHelper.CreateUpdateAsync("master.sp_downtime_log_create_update",param);

                string message = request.downtime_id == null || request.downtime_id == 0
                    ? "Downtime logged successfully."
                    : "Downtime updated successfully.";

                return new OkObjectResult(ResponseHelper<dynamic>.Success(message, result));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Downtime log save error");

                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        ConstantMessages.InternalServerErrorMessage,
                        exception: ex,
                        statusCode: CommonHelper.Enums.StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }
        #endregion
    }
}
