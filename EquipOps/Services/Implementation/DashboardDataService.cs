using CommonHelper.Constants;
using CommonHelper.Enums;
using CommonHelper.Helper;
using CommonHelper.Helpers;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.DashboardData;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EquipOps.Services.Implementation
{
    public class DashboardDataService(IPgHelper pgHelper, ILogger<DashboardDataService> logger) : IDashboardDataService
    {
        public const string OrganizationId = "p_organization_id";

        #region Dashboard Data Rebuild
        public async Task<IActionResult> DashboardRebuildAsync(DashboardRebuildRequest request)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { OrganizationId, new DbParam { Value = request.organization_id, DbType = DbType.Int32 } },
                    { "p_downtime_category_id", new DbParam { Value = request.downtime_category_id, DbType = DbType.Int32 } },
                    { "p_workorder_category_id", new DbParam { Value = request.workorder_category_id, DbType = DbType.Int32 } }
                };

                var result = await pgHelper.CreateUpdateAsync("master.sp_dashboard_rebuild", param);

                return new OkObjectResult(ResponseHelper<dynamic>.Success("Dashboard data rebuilt successfully.",result));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dashboard rebuild error");

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

        #region Dashboard Data Aggregate
        public async Task<IActionResult> DashboardAggregateAsync(DashboardAggregateRequest request)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { OrganizationId, new DbParam { Value = request.organization_id, DbType = DbType.Int32 } },
                    { "p_downtime_cat_id", new DbParam { Value = request.downtime_category_id, DbType = DbType.Int32 } },
                    { "p_workorder_cat_id", new DbParam { Value = request.workorder_category_id, DbType = DbType.Int32 } }
                };

                var result = await pgHelper.CreateUpdateAsync("master.sp_dashboard_aggregate",param);

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success(
                        "Dashboard data aggregated successfully.",
                        result
                    )
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dashboard aggregate error");

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

        #region Dashboard Data Clear
        public async Task<IActionResult> DashboardClearAsync(DashboardClearRequest request)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                {
                    { OrganizationId, new DbParam { Value = request.organization_id, DbType = DbType.Int32 } }
                };

                var result = await pgHelper.CreateUpdateAsync("master.sp_dashboard_clear",param);

                return new OkObjectResult(ResponseHelper<dynamic>.Success("Dashboard data cleared successfully.",result));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dashboard clear error");

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

        #region Dashboard Data List

        public async Task<IActionResult> DashboardDataListAsync(string? search, int length, int page, string orderColumn, string orderDirection)
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
                    { "ref", new DbParam { Value = "dashboard_data_cursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
                };

                dynamic result = await pgHelper.ListAsync("master.sp_dashboard_list", param);

                var list = result.@ref as List<dynamic>;

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Dashboard Data retrieved.", new
                    {
                        TotalNumbers = result.o_total_records,
                        DashboardData = list
                    })
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "List Dashboard Data error");
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

        #region Dashboard KPI Summary

        public async Task<IActionResult> DashboardKpiSummaryAsync(DashboardKpiSummaryRequest request)
        {
            try
            {
                var param = new Dictionary<string, DbParam>
                 {
                     { OrganizationId, new DbParam { Value = request.organization_id, DbType = DbType.Int32 } },
                     {"ref",new DbParam{Value = "kpi_cursor",DbType = DbType.String,Direction = ParameterDirection.InputOutput}}
                };

                dynamic result = await pgHelper.ListAsync("master.sp_dashboard_kpi_summary",param);

                var list = result.@ref as List<dynamic>;
                var kpi = list?.FirstOrDefault();

                return new OkObjectResult(
                    ResponseHelper<dynamic>.Success("Dashboard KPI summary retrieved.", new
                    {
                        TotalDowntime = kpi?.total_downtime ?? 0,
                        TotalFailures = kpi?.total_failures ?? 0,
                        TotalWorkOrders = kpi?.total_work_orders ?? 0,
                        TotalRecords = kpi?.total_records ?? 0
                    })
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dashboard KPI summary error");

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
