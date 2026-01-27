using EquipOps.Model.DashboardData;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Services.Interface
{
    public interface IDashboardDataService
    {
        Task<IActionResult> DashboardRebuildAsync(DashboardRebuildRequest request);
        Task<IActionResult> DashboardAggregateAsync(DashboardAggregateRequest request);
        Task<IActionResult> DashboardClearAsync(DashboardClearRequest request);
        Task<IActionResult> DashboardDataListAsync(string? search, int length, int page, string orderColumn, string orderDirection);
        Task<IActionResult> DashboardKpiSummaryAsync(DashboardKpiSummaryRequest request);
    }
}
