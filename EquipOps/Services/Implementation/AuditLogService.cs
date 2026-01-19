using CommonHelper.constants;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.AuditLog;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text.Json;

namespace EquipOps.Services.Implementation
{
    public class AuditLogService(IPgHelper pgHelper, ILogger<AuditLogService> logger) : IAuditLogService
    {
        public async Task<IActionResult> AuditLogCreateAsync(AuditLogRequest request)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_return_audit_id", new DbParam {DbType = DbType.Int64,Direction = ParameterDirection.InputOutput,Value = 0L}},
                    { "p_organization_id", new DbParam { Value = request.OrganizationId, DbType = DbType.Int32 }},
                    { "p_user_id", new DbParam { Value = request.UserId, DbType = DbType.Int32 }},
                    { "p_entity_name", new DbParam { Value = request.EntityName, DbType = DbType.String }},
                    { "p_entity_id", new DbParam { Value = request.EntityId, DbType = DbType.Int32 }},
                    { "p_action", new DbParam { Value = request.Action, DbType = DbType.String }},
                    { "p_old_data", new DbParam {Value = request.OldData == null ? null : JsonSerializer.Serialize(request.OldData)}},
                    { "p_new_data", new DbParam {Value = request.NewData == null ? null : JsonSerializer.Serialize(request.NewData)}},
                    {"p_ip_address", new DbParam { Value = request.IpAddress, DbType = DbType.String }}
                };

                var result = await pgHelper.CreateUpdateAsync("master.sp_audit_log_create", param);

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Audit log created successfully.", result)
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Audit log save error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(ConstantMessages.InternalServerErrorMessage, exception: ex)
                );
            }
        }

        public async Task<IActionResult> AuditLogByIdAsync(long auditId)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
            {
                { "p_audit_id", new DbParam { Value = auditId, DbType = DbType.Int64 } },
                { "ref", new DbParam { Value = "audit_log_by_id_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
            };

                dynamic result = await pgHelper.ListAsync("master.sp_audit_log_getbyid", param);

                var list = result.@ref as List<dynamic>;

                if (list == null || !list.Any())
                    return new NotFoundObjectResult(
                        ResponseHelper<string>.Error("Audit log not found.")
                    );

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Audit log found.", list[0])
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get Audit log error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(ConstantMessages.InternalServerErrorMessage, exception: ex)
                );
            }
        }

        public async Task<IActionResult> AuditLogListAsync(string? search, string? entityName, string? action, int length, int page)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
            {
                { "p_search", new DbParam { Value = search, DbType = DbType.String } },
                { "p_entity_name", new DbParam { Value = entityName, DbType = DbType.String } },
                { "p_action", new DbParam { Value = action, DbType = DbType.String } },
                { "p_length", new DbParam { Value = length, DbType = DbType.Int32 } },
                { "p_page", new DbParam { Value = page, DbType = DbType.Int32 } },
                { "o_total_records", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                { "ref", new DbParam { Value = "audit_log_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
            };

                dynamic result = await pgHelper.ListAsync("master.sp_audit_log_list", param);

                var list = result.@ref as List<dynamic>;

                if (list == null || !list.Any())
                    return new NotFoundObjectResult(
                        ResponseHelper<string>.Error("No audit logs found.")
                    );

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Audit logs retrieved.", new
                    {
                        TotalRecords = result.o_total_records,
                        AuditData = list
                    })
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "List Audit logs error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(ConstantMessages.InternalServerErrorMessage, exception: ex)
                );
            }
        }
    }
}
