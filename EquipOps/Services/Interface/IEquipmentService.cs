using EquipOps.Model.Requests.Equipment;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Services.Implementation
{
	public interface IEquipmentService
	{
		Task<IActionResult> AddOrUpdateAsync(EquipmentRequest request);
		Task<IActionResult> GetByIdAsync(int equipmentId);
		Task<IActionResult> GetEquipmentsAsync(string? search,int length,int page,string orderColumn,string orderDirection,int? isActive);
		Task<IActionResult> DeleteAsync(int equipmentId);
	}

}
