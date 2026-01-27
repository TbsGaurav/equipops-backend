using EquipOps.Model.DashboardData;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class DashboardDataController(IDashboardDataService _dashboardDataService) : ControllerBase
    {
        [HttpPost("clear")]
        public async Task<IActionResult> Clear([FromBody] DashboardClearRequest request)
        {
            return Ok(await _dashboardDataService.DashboardClearAsync(request));
        }

        [HttpPost("aggregate")]
        public async Task<IActionResult> Aggregate([FromBody] DashboardAggregateRequest request)
        {
            return Ok(await _dashboardDataService.DashboardAggregateAsync(request));
        }

        [HttpPost("rebuild")]
        public async Task<IActionResult> Rebuild([FromBody] DashboardRebuildRequest request)
        {
            return Ok(await _dashboardDataService.DashboardRebuildAsync(request));
        }
        [HttpGet("dashboardList")]
        public async Task<IActionResult> GetDashboardDataList(string? search = "", int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "ASC")
        {
            var result = await _dashboardDataService.DashboardDataListAsync(search, length, page, orderColumn, orderDirection);
            return Ok(result);
        }

        [HttpPost("KPISummary")]
        public async Task<IActionResult> DashboardKpiSummaryAsync(DashboardKpiSummaryRequest request)
        {
            return await _dashboardDataService.DashboardKpiSummaryAsync(request);
        }
    }
}
