using EquipOps.Model.SlaMetrics;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Services.Interface
{
	public interface ISlaMetricsService
	{
		Task<IActionResult> AddOrUpdateAsync(SlaMetricsRequest request);
		Task<IActionResult> GetByIdAsync(int slaId);
		Task<IActionResult> GetPagedAsync(int organizationId, DateTime startDate, DateTime endDate, bool? slaBreached, int page, int length);
		Task<IActionResult> GetDashboardSummaryAsync(int organizationId, DateTime startDate, DateTime endDate);
		Task<IActionResult> DeleteAsync(int slaId);
	}
}
