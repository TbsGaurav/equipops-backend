using EquipOps.Model.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Services.Interface
{
	public interface IDashboardCategoryService
	{
		Task<IActionResult> AddOrUpdateAsync(DashboardCategoryRequest request);
		Task<IActionResult> GetByIdAsync(int id);
		Task<IActionResult> GetListAsync(string? search, int length, int page, string orderColumn, string orderDirection);
		Task<IActionResult> DeleteAsync(int id);
	}

}
