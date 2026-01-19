using CommonHelper.Enums;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.Dashboard;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EquipOps.Services.Implementation
{
	public sealed class DashboardCategoryService : IDashboardCategoryService
	{
		private readonly IPgHelper _pgHelper;
		private readonly ILogger<DashboardCategoryService> _logger;

		public DashboardCategoryService(IPgHelper pgHelper, ILogger<DashboardCategoryService> logger)
		{
			_pgHelper = pgHelper;
			_logger = logger;
		}

		public async Task<IActionResult> AddOrUpdateAsync(DashboardCategoryRequest request)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
				{
					{ "p_dashboard_category_id", new DbParam { Value = request.DashboardCategoryId, DbType = DbType.Int32 } },
					{ "p_organization_id", new DbParam { Value = request.OrganizationId, DbType = DbType.Int32 } },
					{ "p_name", new DbParam { Value = request.Name, DbType = DbType.String } },
					{ "p_description", new DbParam { Value = request.Description, DbType = DbType.String } }
				};

				var result = await _pgHelper.CreateUpdateAsync("master.sp_dashboard_category_add_update", param);

				return new OkObjectResult(ResponseHelper<dynamic>.Success("Dashboard category saved.", result));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Dashboard category save error");
				return new ObjectResult(ResponseHelper<string>.Error(
					"Internal server error.", exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
			}
		}

		public async Task<IActionResult> GetByIdAsync(int id)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
				{
					{ "p_dashboard_category_id", new DbParam { Value = id, DbType = DbType.Int32 } },
					{ "ref", new DbParam { Value = "dashboard_category_by_id_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
				};

				dynamic result = await _pgHelper.ListAsync("master.sp_dashboard_category_get_by_id", param);
				var list = result.@ref as List<dynamic>;

				if (list == null || !list.Any())
					return new NotFoundObjectResult(ResponseHelper<string>.Error("Category not found.", statusCode: StatusCodeEnum.NOT_FOUND));

				return new OkObjectResult(ResponseHelper<dynamic>.Success("Category found.", list.First()));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Get dashboard category error");
				return new ObjectResult(ResponseHelper<string>.Error(
					"Internal server error.", exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
			}
		}

		public async Task<IActionResult> GetListAsync(string? search, int length, int page, string orderColumn, string orderDirection)
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
					{ "ref", new DbParam { Value = "dashboard_category_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
				};

				dynamic result = await _pgHelper.ListAsync("master.sp_dashboard_category_list_get", param);
				var list = result.@ref as List<dynamic>;

				if (list == null || !list.Any())
					return new NotFoundObjectResult(ResponseHelper<string>.Error("No category found.", statusCode: StatusCodeEnum.NOT_FOUND));

				return new OkObjectResult(ResponseHelper<dynamic>.Success("Categories retrieved.", new
				{
					TotalNumbers = result.o_total_numbers,
					Data = list
				}));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "List dashboard category error");
				return new ObjectResult(ResponseHelper<string>.Error(
					"Internal server error.", exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
			}
		}

		public async Task<IActionResult> DeleteAsync(int id)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
				{
					{ "p_dashboard_category_id", new DbParam { Value = id, DbType = DbType.Int32 } }
				};

				await _pgHelper.CreateUpdateAsync("master.sp_dashboard_category_delete", param);
				return new OkObjectResult(ResponseHelper<bool>.Success("Category deleted.", true));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Delete dashboard category error");
				return new ObjectResult(ResponseHelper<string>.Error(
					"Internal server error.", exception: ex, statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR));
			}
		}
	}
}
