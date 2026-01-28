using EquipOps.Model.EquipmentFailure;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Services.Interface
{
    public interface IEquipmentFailureService
    {
        Task<IActionResult> EquipmentFailureCreateUpdateAsync(EquipmentFailureRequest request);
        Task<IActionResult> EquipmentFailureByIdAsync(int failure_id);
        Task<IActionResult> EquipmentFailureDeleteAsync(int failure_id);
        Task<IActionResult> EquipmentFailureListAsync(string? search,int length, int page,string orderColumn,string orderDirection);
    }
}
