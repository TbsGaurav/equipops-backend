using CommonHelper.constants;
using CommonHelper.Enums;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.SlaMetrics;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EquipOps.Services.Implementation
{
	public sealed class SlaMetricsService(IPgHelper pgHelper, ILogger<SlaMetricsService> logger)
		: ISlaMetricsService
	{
		#region Create / Update
		public async Task<IActionResult> AddOrUpdateAsync(SlaMetricsRequest request)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
				{

					{ "p_sla_id", new DbParam { Value = request.SlaId ?? 0, DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
					{ "p_organization_id", new DbParam { Value = request.OrganizationId, DbType = DbType.Int32 } },
					{ "p_equipment_id", new DbParam { Value = request.EquipmentId, DbType = DbType.Int32 } },
					{ "p_subpart_id", new DbParam { Value = request.SubpartId, DbType = DbType.Int32 } },
					{ "p_period_start", new DbParam { Value = request.PeriodStart.Date, DbType = DbType.Date } },
					{ "p_period_end", new DbParam { Value = request.PeriodEnd.Date, DbType = DbType.Date } },
					{ "p_downtime_minutes", new DbParam { Value = request.DowntimeMinutes, DbType = DbType.Int32 } },
					{ "p_sla_breached", new DbParam { Value = request.SlaBreached, DbType = DbType.Boolean } }
				};

				var result = await pgHelper.CreateUpdateAsync("master.sp_sla_metrics_create_update", param);

				return new OkObjectResult(
					ResponseHelper<dynamic>.Success("SLA Metrics saved successfully.", result)
				);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "SLA Metrics save error");
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

		#region Get By Id
		public async Task<IActionResult> GetByIdAsync(int slaId)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
				{
					{ "p_sla_id", new DbParam { Value = slaId, DbType = DbType.Int32 } },
					{ "ref", new DbParam { Value = "sla_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
				};

				dynamic result = await pgHelper.ListAsync("master.sp_sla_metrics_get_by_id", param);
				var list = result.@ref as List<dynamic>;

				if (list == null || !list.Any())
					return new NotFoundObjectResult(ResponseHelper<string>.Error("SLA record not found.", statusCode: StatusCodeEnum.NOT_FOUND));

				return new OkObjectResult(ResponseHelper<dynamic>.Success("SLA record found.", list[0]));
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Get SLA error");
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

		#region Get Paged List
		public async Task<IActionResult> GetPagedAsync(int organizationId, DateTime startDate, DateTime endDate, bool? slaBreached, int page, int length)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
				{
					{ "p_organization_id", new DbParam { Value = organizationId, DbType = DbType.Int32 } },
					{ "p_start_date", new DbParam { Value = startDate.Date, DbType = DbType.Date } },
					{ "p_end_date", new DbParam { Value = endDate.Date, DbType = DbType.Date } },
					{ "p_sla_breached", new DbParam { Value = (object?)slaBreached ?? DBNull.Value, DbType = DbType.Boolean } },
					{ "p_page_number", new DbParam { Value = page, DbType = DbType.Int32 } },
					{ "p_page_size", new DbParam { Value = length, DbType = DbType.Int32 } },
					{ "ref", new DbParam { Value = "sla_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
				};

				dynamic result = await pgHelper.ListAsync("master.sp_sla_metrics_get_paged", param);
				var list = result.@ref as List<dynamic> ?? new List<dynamic>();

				return new OkObjectResult(ResponseHelper<dynamic>.Success("SLA list loaded.", list));
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "SLA list error");
				return new ObjectResult(
					ResponseHelper<string>.Error(
						ConstantMessages.InternalServerErrorMessage,
						exception: ex
					)
				);
			}
		}
		#endregion

		#region Dashboard Summary
		public async Task<IActionResult> GetDashboardSummaryAsync(int organizationId, DateTime startDate, DateTime endDate)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
				{
					{ "p_organization_id", new DbParam { Value = organizationId, DbType = DbType.Int32 } },
					{ "p_start_date", new DbParam { Value = startDate.Date, DbType = DbType.Date } },
					{ "p_end_date", new DbParam { Value = endDate.Date, DbType = DbType.Date } },
					{ "ref", new DbParam { Value = "summary_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
				};

				dynamic result = await pgHelper.ListAsync("master.sp_sla_metrics_dashboard_summary", param);
				var list = result.@ref as List<dynamic>;

				return new OkObjectResult(ResponseHelper<dynamic>.Success("Dashboard summary loaded.", list?.FirstOrDefault()));
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Dashboard summary error");
				return new ObjectResult(ResponseHelper<string>.Error(
					ConstantMessages.InternalServerErrorMessage,
					exception: ex
				));
			}
		}
		#endregion

		#region Delete
		public async Task<IActionResult> DeleteAsync(int slaId)
		{
			try
			{
				var param = new Dictionary<string, DbParam>
				{
					{ "p_sla_id", new DbParam { Value = slaId, DbType = DbType.Int32 } }
				};

				await pgHelper.CreateUpdateAsync("master.sp_sla_metrics_delete", param);
				return new OkObjectResult(ResponseHelper<bool>.Success("SLA deleted successfully.", true));
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Delete SLA error");
				return new ObjectResult(ResponseHelper<string>.Error(
					ConstantMessages.InternalServerErrorMessage,
					exception: ex,
					statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
				));
			}
		}
		#endregion
	}
}
