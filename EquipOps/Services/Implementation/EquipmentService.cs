using CommonHelper.constants;
using CommonHelper.Enums;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.Equipment;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EquipOps.Services.Implementation
{
	public sealed class EquipmentService(IPgHelper pgHelper, ILogger<EquipmentService> logger) : IEquipmentService
	{
        #region Create/Update Equipment
        public async Task<IActionResult> AddOrUpdateAsync(EquipmentRequest request)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
		        {
		        	{ "p_equipment_id",   new DbParam { Value = request.EquipmentId,   DbType = DbType.Int32 } },
		        	{ "p_organization_id",new DbParam { Value = request.OrganizationId,DbType = DbType.Int32 } },
		        	{ "p_category_id",    new DbParam { Value = request.CategoryId,    DbType = DbType.Int32 } },
		        	{ "p_name",           new DbParam { Value = request.Name,          DbType = DbType.String } },
		        	{ "p_type",           new DbParam { Value = (object?)request.Type ?? DBNull.Value,     DbType = DbType.String } },
		        	{ "p_qr_code",        new DbParam { Value = (object?)request.QrCode ?? DBNull.Value,   DbType = DbType.String } },
		        	{ "p_location",       new DbParam { Value = (object?)request.Location ?? DBNull.Value,DbType = DbType.String } },
		        	{ "p_purchase_date",  new DbParam { Value = (object?)request.PurchaseDate?.Date ?? DBNull.Value, DbType = DbType.Date } },
		        	{ "p_status",         new DbParam { Value = request.Status,        DbType = DbType.Int32 } }
		        };
				var result = await pgHelper.CreateUpdateAsync(
					"master.sp_equipment_add_update",
					param
				);
				return new OkObjectResult(
					ResponseHelper<dynamic>.Success("Equipment saved successfully.", result)
				);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Equipment save error");
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

		#region  Get Equipment By Id
		public async Task<IActionResult> GetByIdAsync(int equipmentId)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
				{
					{ "p_equipment_id", new DbParam { Value = equipmentId, DbType = DbType.Int32 } },
					{ "ref", new DbParam { Value = "equipment_by_id_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
				};

				dynamic result = await pgHelper.ListAsync("master.sp_equipment_get_by_id", param);
				var list = result.@ref as List<dynamic>;

				if (list == null || !list.Any())
					return new NotFoundObjectResult(ResponseHelper<string>.Error("Equipment not found.", statusCode: StatusCodeEnum.NOT_FOUND));

				return new OkObjectResult(ResponseHelper<dynamic>.Success("Equipment found.", list[0]));
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Get Equipment error");
				return new ObjectResult(ResponseHelper<string>.Error(
					ConstantMessages.InternalServerErrorMessage, exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
			}
		}
		#endregion

		#region  Get Equipment List
		public async Task<IActionResult> GetEquipmentsAsync(string? search,	int length,	int page,string orderColumn,string orderDirection,int? isActive )
		{
			try
			{
				var param = new Dictionary<string, DbParam>
		        {
		        	{ "p_search", new DbParam { Value = (object?)search ?? DBNull.Value, DbType = DbType.String } },
		        	{ "p_length", new DbParam { Value = length, DbType = DbType.Int32 } },
		        	{ "p_page", new DbParam { Value = page, DbType = DbType.Int32 } },
		        	{ "p_order_column", new DbParam { Value = orderColumn, DbType = DbType.String } },
		        	{ "p_order_direction", new DbParam { Value = orderDirection, DbType = DbType.String } },
		        	{ "p_is_active", new DbParam { Value = isActive ?? -1, DbType = DbType.Int32 } }, 
                    { "o_total_numbers", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
		        	{ "ref", new DbParam { Value = "equipment_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
		        };
				dynamic result = await pgHelper.ListAsync("master.sp_equipment_list_get", param);
				var list = result.@ref as List<dynamic>;
				if (list == null || !list.Any())
					return new NotFoundObjectResult(ResponseHelper<string>.Error("No equipment found."));

				return new OkObjectResult(ResponseHelper<dynamic>.Success("Equipments retrieved.", new
				{
					TotalNumbers = result.o_total_numbers,
					EquipmentData = list
				}));
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "List Equipment error");
				return new ObjectResult(ResponseHelper<string>.Error(
					ConstantMessages.InternalServerErrorMessage, exception: ex));
			}
		}
		#endregion

		#region Delete Equipment
		public async Task<IActionResult> DeleteAsync(int equipmentId)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
				{
					{ "p_equipment_id", new DbParam { Value = equipmentId, DbType = DbType.Int32 } }
				};

				await pgHelper.CreateUpdateAsync("master.sp_equipment_delete", param);
				return new OkObjectResult(ResponseHelper<bool>.Success("Equipment deleted.", true));
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Delete Equipment error");
				return new ObjectResult(ResponseHelper<string>.Error(
					ConstantMessages.InternalServerErrorMessage, exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
			}
		}

        #endregion

        #region Equipment Dropdown
        public async Task<IActionResult> EquipmentDropdownAsync()
        {
            try
            {
                var param = new Dictionary<string, DbParam>
        {
            {
                "ref",
                new DbParam
                {
                    Value = "equipment_cursor",
                    DbType = DbType.String,
                    Direction = ParameterDirection.InputOutput
                }
            }
        };

                dynamic result = await pgHelper.ListAsync("master.sp_equipment_dropdown", param);

                var list = result?.@ref as List<dynamic> ?? new List<dynamic>();

                return new OkObjectResult(ResponseHelper<dynamic>.Success("Equipment loaded.", list));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Equipment dropdown error");
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