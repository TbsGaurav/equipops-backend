using CommonHelper.Enums;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.Requests.Equipment;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EquipOps.Serives.Implementation
{
	public sealed class EquipmentService : IEquipmentService
	{
		private readonly IPgHelper _pgHelper;
		private readonly ILogger<EquipmentService> _logger;

		public EquipmentService(IPgHelper pgHelper, ILogger<EquipmentService> logger)
		{
			_pgHelper = pgHelper;
			_logger = logger;
		}

		public async Task<IActionResult> AddOrUpdateAsync(EquipmentRequest request)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
                {
                    { "p_equipment_id", new DbParam { Value = request.EquipmentId, DbType = DbType.Int32 } },
                    { "p_organization_id", new DbParam { Value = request.OrganizationId, DbType = DbType.Int32 } },
                    { "p_category_id", new DbParam { Value = request.CategoryId, DbType = DbType.Int32 } },
                    { "p_name", new DbParam { Value = request.Name, DbType = DbType.String } },
                    { "p_type", new DbParam { Value = request.Type, DbType = DbType.String } },
                    { "p_qr_code", new DbParam { Value = request.QrCode, DbType = DbType.String } },
                    { "p_location", new DbParam { Value = request.Location, DbType = DbType.String } },
                    { "p_purchase_date", new DbParam { Value = request.PurchaseDate?.Date, DbType = DbType.Date }},
                    { "p_status", new DbParam { Value = request.Status, DbType = DbType.Int32 } }
                };

				var result = await _pgHelper.CreateUpdateAsync("master.sp_equipment_add_update", param);

				return new OkObjectResult(ResponseHelper<dynamic>.Success("Equipment saved successfully.", result));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Equipment save error");
				return new ObjectResult(ResponseHelper<string>.Error(
					"Internal server error.", exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
			}
		}

		public async Task<IActionResult> GetByIdAsync(int equipmentId)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
				{
					{ "p_equipment_id", new DbParam { Value = equipmentId, DbType = DbType.Int32 } },
					{ "ref", new DbParam { Value = "equipment_by_id_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
				};

				dynamic result = await _pgHelper.ListAsync("master.sp_equipment_get_by_id", param);
				var list = result.@ref as List<dynamic>;

				if (list == null || !list.Any())
					return new NotFoundObjectResult(ResponseHelper<string>.Error("Equipment not found.", statusCode: StatusCodeEnum.NOT_FOUND));

				return new OkObjectResult(ResponseHelper<dynamic>.Success("Equipment found.", list.First()));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Get Equipment error");
				return new ObjectResult(ResponseHelper<string>.Error(
					"Internal server error.", exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
			}
		}

		public async Task<IActionResult> GetEquipmentsAsync(string? search, int length, int page, string orderColumn, string orderDirection)
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
					{ "o_total_numbers", new DbParam { DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
					{ "ref", new DbParam { Value = "equipment_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
				};

				dynamic result = await _pgHelper.ListAsync("master.sp_equipment_list_get", param);
				var list = result.@ref as List<dynamic>;

				if (list == null || !list.Any())
					return new NotFoundObjectResult(ResponseHelper<string>.Error("No equipment found.", statusCode: StatusCodeEnum.NOT_FOUND));

				return new OkObjectResult(ResponseHelper<dynamic>.Success("Equipments retrieved.", new
				{
					TotalNumbers = result.o_total_numbers,
					EquipmentData = list
				}));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "List Equipment error");
				return new ObjectResult(ResponseHelper<string>.Error(
					"Internal server error.", exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
			}
		}

		public async Task<IActionResult> DeleteAsync(int equipmentId)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
				{
					{ "p_equipment_id", new DbParam { Value = equipmentId, DbType = DbType.Int32 } }
				};

				await _pgHelper.CreateUpdateAsync("master.sp_equipment_delete", param);
				return new OkObjectResult(ResponseHelper<bool>.Success("Equipment deleted.", true));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Delete Equipment error");
				return new ObjectResult(ResponseHelper<string>.Error(
					"Internal server error.", exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
			}
		}
	}
}
