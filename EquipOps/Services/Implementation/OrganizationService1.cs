using CommonHelper.Constants;
using CommonHelper.Enums;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.Organization;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EquipOps.Services.Implementation
{
    public class OrganizationService1(IPgHelper pgHelper, ILogger<OrganizationService1> logger) : IOrganizationService1
    {
        public async Task<IActionResult> OrganizationCreateAsync(Organization1Request request)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_return_organization_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                    { "p_return_updated_at", new DbParam { DbType = DbType.DateTime, Direction = ParameterDirection.InputOutput } },
                    { "p_organization_id", new DbParam { Value = request.organization_id, DbType = DbType.Int32 } },
                    { "p_name", new DbParam { Value = request.name, DbType = DbType.String } },
                    { "p_address", new DbParam { Value = request.address, DbType = DbType.String } },
                    { "p_contact_email", new DbParam { Value = request.contact_email, DbType = DbType.String } },
                    { "p_contact_phone", new DbParam { Value = request.contact_phone, DbType = DbType.String } }
                };

                var result = await pgHelper.CreateUpdateAsync("master.sp_organization_create_update",param);

                string message = request.organization_id == null || request.organization_id == 0
                    ? "Organization created successfully."
                    : "Organization updated successfully.";

                return new OkObjectResult(ResponseHelper<dynamic>.Success(message, result));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Organization save error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        ConstantMessages.InternalServerErrorMessage,
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        public async Task<IActionResult> OrganizationByIdAsync(int organization_id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_organization_id", new DbParam { Value = organization_id, DbType = DbType.Int32 } },
                    { "ref", new DbParam { Value = "organization_by_id_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
                };

                dynamic result = await pgHelper.ListAsync("master.sp_organization_getbyid",param);

                var list = result.@ref as List<dynamic>;

                if (list == null || !list.Any())
                    return new NotFoundObjectResult(
                        ResponseHelper<string>.Error(
                            "Organization not found.",
                            statusCode: StatusCodeEnum.NOT_FOUND
                        )
                    );

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Organization found.", list[0])
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Get Organization error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        ConstantMessages.InternalServerErrorMessage,
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        public async Task<IActionResult> OrganizationDeleteAsync(int organization_id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_return_organization_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                    { "p_return_updated_at", new DbParam { DbType = DbType.DateTime, Direction = ParameterDirection.InputOutput } },
                    { "p_organization_id", new DbParam { Value = organization_id, DbType = DbType.Int32 } }
                };

                var result = await pgHelper.CreateUpdateAsync("master.sp_organization_delete",param);

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Organization deleted successfully.", result)
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Delete Organization error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        ConstantMessages.InternalServerErrorMessage,
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }

        public async Task<IActionResult> OrganizationListAsync(string? search, int length, int page, string orderColumn, string orderDirection)
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
                    { "ref", new DbParam { Value = "organization_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
                };

                dynamic result = await pgHelper.ListAsync("master.sp_organization_list",param);

                var list = result.@ref as List<dynamic>;

                if (list == null || !list.Any())
                    return new NotFoundObjectResult(
                        ResponseHelper<string>.Error(
                            "No organizations found.",
                            statusCode: StatusCodeEnum.NOT_FOUND
                        )
                    );

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Organizations retrieved.", new
                    {
                        TotalNumbers = result.o_total_records,
                        OrganizationData = list
                    })
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "List Organization error");
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        ConstantMessages.InternalServerErrorMessage,
                        exception: ex,
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }
        }
        public async Task<IActionResult> OrganizationDropdownAsync()
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    {
                        "ref",
                        new DbParam
                        {
                            Value = "org_cursor",
                            DbType = DbType.String,
                            Direction = ParameterDirection.InputOutput
                        }
                    }
                };

                dynamic result = await pgHelper.ListAsync("master.sp_organization_dropdown",param);

                var list = result?.@ref as List<dynamic> ?? new List<dynamic>();

                return new OkObjectResult(ResponseHelper<dynamic>.Success("Organizations loaded.", list));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Organization dropdown error");
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
