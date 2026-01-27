using CommonHelper.Constants;
using CommonHelper.Enums;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.Role;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EquipOps.Services.Implementation
{
    public class RoleService(IPgHelper pgHelper, ILogger<RoleService> logger) : IRoleService
    {
        #region Create / Update Role

        public async Task<IActionResult> RoleCreateUpdateAsync(RoleRequest request)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_role_id", new DbParam { Value = request.role_id, DbType = DbType.Int32 } },
                    { "p_description", new DbParam { Value = request.description, DbType = DbType.String } },
                    { "p_created_by", new DbParam { Value = request.created_by, DbType = DbType.Guid } },
                    { "p_role_name", new DbParam { Value = request.role_name, DbType = DbType.String } },
                    { "p_is_active", new DbParam { Value = request.is_active, DbType = DbType.Boolean } },
      
                    { "o_role_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.Output } },
                    { "o_created_date", new DbParam { DbType = DbType.DateTime2, Direction = ParameterDirection.Output } },
                    { "o_updated_date", new DbParam { DbType = DbType.DateTime2, Direction = ParameterDirection.Output } }
                };

                var result = await pgHelper.CreateUpdateAsync("master.sp_role_create_update",param);

                string message = request.role_id == null
                    ? "Role created successfully."
                    : "Role updated successfully.";

                return new OkObjectResult(ResponseHelper<dynamic>.Success(message, result));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Role create/update error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        ConstantMessages.InternalServerErrorMessage,
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        #endregion

        #region Get Role By Id

        public async Task<IActionResult> RoleByIdAsync(int role_id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_role_id", new DbParam { Value = role_id, DbType = DbType.Int32 } },
                    { "ref", new DbParam { Value = "role_by_id_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
                };

                dynamic result = await pgHelper.ListAsync("master.sp_role_getbyid",param);

                var list = result.@ref as List<dynamic>;

                if (list == null || !list.Any())
                {
                    return new NotFoundObjectResult(
                        ResponseHelper<string>.Error(
                            "Role not found.",
                            statusCode: StatusCodeEnum.NOT_FOUND
                        )
                    );
                }

                return new OkObjectResult(ResponseHelper<dynamic>.Success("Role found.", list[0]));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get role by id error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        ConstantMessages.InternalServerErrorMessage,
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        #endregion

        #region Role List

        public async Task<IActionResult> RoleListAsync(string? search,bool? is_active,int length,int page,string orderColumn,string orderDirection)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_search", new DbParam { Value = search, DbType = DbType.String } },
                    { "p_is_active", new DbParam { Value = is_active, DbType = DbType.Boolean } },
                    { "p_length", new DbParam { Value = length, DbType = DbType.Int32 } },
                    { "p_page", new DbParam { Value = page, DbType = DbType.Int32 } },
                    { "p_order_column", new DbParam { Value = orderColumn, DbType = DbType.String } },
                    { "p_order_direction", new DbParam { Value = orderDirection, DbType = DbType.String } },
                    { "o_total_records", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                    { "ref", new DbParam { Value = "role_list_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
                };

                dynamic result = await pgHelper.ListAsync("master.sp_role_list",param);

                var list = result.@ref as List<dynamic>;

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Roles retrieved.", new
                    {
                        TotalRecords = result.o_total_records,
                        RoleData = list
                    })
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Role list error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        ConstantMessages.InternalServerErrorMessage,
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        #endregion

        #region Delete Role

        public async Task<IActionResult> RoleDeleteAsync(int role_id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_return_role_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                    { "p_return_updateddate", new DbParam { DbType = DbType.DateTime, Direction = ParameterDirection.InputOutput } },
                    { "p_role_id", new DbParam { Value = role_id, DbType = DbType.Int32 } },
                    { "p_is_delete", new DbParam { Value = true, DbType = DbType.Boolean } },
                    { "p_is_active", new DbParam { Value = false, DbType = DbType.Boolean } }
                };

                var result = await pgHelper.CreateUpdateAsync("master.sp_role_delete",param);

                return new OkObjectResult(ResponseHelper<dynamic>.Success("Role deleted successfully.", result));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Role delete error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        ConstantMessages.InternalServerErrorMessage,
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        #endregion
    }
}