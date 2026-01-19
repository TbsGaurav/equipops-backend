using EquipOps.Model.EquipmentSubpart;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Services.Interface
{
    public interface IEquipmentSubpartService
    {
        Task<IActionResult> EquipmentSubpartCreateUpdateAsync(EquipmentSubpartRequest request);
        Task<IActionResult> EquipmentSubpartByIdAsync(int subpart_id);
        Task<IActionResult> EquipmentSubpartDeleteAsync(int subpart_id);
        Task<IActionResult> EquipmentSubpartListAsync(string? search, bool? status, int length, int page, string orderColumn, string orderDirection);
        Task<IActionResult> EquipmentSubpartDropdownAsync();
    }
}
