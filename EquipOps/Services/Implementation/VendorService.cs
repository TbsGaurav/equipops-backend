using CommonHelper.Enums;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.Vendor;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EquipOps.Services.Implementation
{
    public sealed class VendorService : IVendorService
    {
        private readonly IPgHelper _pgHelper;
        private readonly ILogger<VendorService> _logger;

        public VendorService(IPgHelper pgHelper, ILogger<VendorService> logger)
        {
            _pgHelper = pgHelper;
            _logger = logger;
        }

        public async Task<IActionResult> VendorCreateAsync(VendorRequest request)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_vendor_id", new DbParam { Value = request.vendor_id, DbType = DbType.Int32 } },
                    { "p_organization_id", new DbParam { Value = request.organization_id, DbType = DbType.Int32 } },
                    { "p_name", new DbParam { Value = request.name, DbType = DbType.String } },
                    { "p_contact_name", new DbParam { Value = request.contact_name, DbType = DbType.String } },
                    { "p_email", new DbParam { Value = request.email, DbType = DbType.String } },
                    { "p_phone", new DbParam { Value = request.phone, DbType = DbType.String } },
                    { "p_service_type", new DbParam { Value = request.service_type, DbType = DbType.String } },
                    { "p_return_vendor_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                    { "p_return_updated_at", new DbParam { DbType = DbType.DateTime, Direction = ParameterDirection.InputOutput } }
                };

                var result = await _pgHelper.CreateUpdateAsync("master.sp_vendor_create_update", param);
                return new OkObjectResult(ResponseHelper<dynamic>.Success("Vendor saved successfully.", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Vendor save error");
                return new ObjectResult(ResponseHelper<string>.Error(
                    "Internal server error.", exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
            }
        }

        public async Task<IActionResult> VendorByIdAsync(int vendor_id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_vendor_id", new DbParam { Value = vendor_id, DbType = DbType.Int32 } },
                    { "ref", new DbParam { Value = "vendor_by_id_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
                };

                dynamic result = await _pgHelper.ListAsync("master.sp_vendor_getbyid", param);
                var list = result.@ref as List<dynamic>;

                if (list == null || !list.Any())
                    return new NotFoundObjectResult(ResponseHelper<string>.Error("Vendor not found.", statusCode: StatusCodeEnum.NOT_FOUND));

                return new OkObjectResult(ResponseHelper<dynamic>.Success("Vendor found.", list.First()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get Vendor error");
                return new ObjectResult(ResponseHelper<string>.Error(
                    "Internal server error.", exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
            }
        }

        public async Task<IActionResult> VendorDeleteAsync(int id)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { "p_vendor_id", new DbParam { Value = id, DbType = DbType.Int32 } },
                    { "p_return_vendor_id", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } }
                };

                var result = await _pgHelper.CreateUpdateAsync("master.sp_vendor_delete", param);
                return new OkObjectResult(ResponseHelper<dynamic>.Success("Vendor deleted.", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Vendor error");
                return new ObjectResult(ResponseHelper<string>.Error(
                    "Internal server error.", exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
            }
        }

        public async Task<IActionResult> VendorListAsync(string? search, int length, int page, string orderColumn, string orderDirection)
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
                    { "ref", new DbParam { Value = "vendor_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
                };

                dynamic result = await _pgHelper.ListAsync("master.sp_vendor_list", param);
                var list = result.@ref as List<dynamic>;

                if (list == null || !list.Any())
                    return new NotFoundObjectResult(ResponseHelper<string>.Error("No vendors found.", statusCode: StatusCodeEnum.NOT_FOUND));

                return new OkObjectResult(ResponseHelper<dynamic>.Success("Vendors retrieved.", new
                {
                    TotalNumbers = result.o_total_records,
                    VendorData = list
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "List Vendor error");
                return new ObjectResult(ResponseHelper<string>.Error(
                    "Internal server error.", exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
            }
        }
    }
}
