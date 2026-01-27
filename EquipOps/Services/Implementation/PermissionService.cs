using CommonHelper.Constants;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.Permission;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EquipOps.Services.Implementation
{
    public class PermissionService (IPgHelper pgHelper, ILogger<PermissionService> logger) : IPermissionService
    {        
        #region Create/Update Permission

        public async Task<dynamic> PermissionCreateUpdateAsync(PermissionRequest request)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_permission_id", new DbParam { Value = request.permission_id ?? 0, DbType = DbType.Int32 } },
                    { "p_permission_code", new DbParam { Value = request.permission_code ?? "", DbType = DbType.String } },
                    { "p_description", new DbParam { Value = request.description ?? "", DbType = DbType.String } },
                    { "p_is_active", new DbParam { Value = request.is_active, DbType = DbType.Boolean } },
                    { "o_permission_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.Output } }
                };

                var result = await pgHelper.CreateUpdateAsync("master.sp_permission_create_update",param);

                string message = request.permission_id == null || request.permission_id == 0
                    ? "Permission created successfully."
                    : "Permission updated successfully.";

                return ResponseHelper<dynamic>.Success(message, result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Permission save error");
                return ResponseHelper<string>.Error(ConstantMessages.InternalServerErrorMessage, exception: ex);
            }
        }
        #endregion

        #region Permission Get By Id
        public async Task<dynamic> PermissionByIdAsync(int permission_id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
            {
                { "p_permission_id", new DbParam { Value = permission_id, DbType = DbType.Int32 } },
                { "p_cursor", new DbParam { Value = "permission_by_id_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
            };

                dynamic result = await pgHelper.ListAsync("master.sp_permission_get_by_id",param);

                var list = result.p_cursor as List<dynamic>;

                if (list == null || !list.Any())
                    return ResponseHelper<string>.Error("Permission not found.");

                return ResponseHelper<dynamic>.Success("Permission found.", list[0]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get permission error");
                return ResponseHelper<string>.Error(ConstantMessages.InternalServerErrorMessage, exception: ex);
            }
        }
        #endregion

        #region Permission List

        public async Task<IActionResult> PermissionListAsync(string? search,bool? status,int length,int page,string orderColumn,string orderDirection)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
        {
            { "p_search", new DbParam { Value = search ?? "", DbType = DbType.String } },
            { "p_status", new DbParam { Value = status, DbType = DbType.Boolean } },
            { "p_length", new DbParam { Value = length, DbType = DbType.Int32 } },
            { "p_page", new DbParam { Value = page, DbType = DbType.Int32 } },
            { "p_order_column", new DbParam { Value = orderColumn, DbType = DbType.String } },
            { "p_order_direction", new DbParam { Value = orderDirection, DbType = DbType.String } },
            { "o_total_records", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
            { "ref", new DbParam { Value = "permission_list_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
        };

                dynamic result = await pgHelper.ListAsync("master.sp_permission_list",param);

                var list = result.@ref as List<dynamic>;

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Permissions retrieved.", new
                    {
                        TotalNumbers = result.o_total_records,
                        PermissionData = list
                    })
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "List Permission error");
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

        #region Permission Delete
         
        public async Task<dynamic> PermissionDeleteAsync(int permission_id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
            {
                { "p_permission_id", new DbParam { Value = permission_id, DbType = DbType.Int32 } }
            };

                var result = await pgHelper.CreateUpdateAsync("master.sp_permission_delete",param);

                return ResponseHelper<dynamic>.Success("Permission deleted successfully.", result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Permission delete error");
                return ResponseHelper<string>.Error(ConstantMessages.InternalServerErrorMessage, exception: ex);
            }
        }
        #endregion
    }
}
