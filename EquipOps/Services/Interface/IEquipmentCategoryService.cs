using EquipOps.Model.EquipmentCategory;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Services.Interface
{
    public interface IEquipmentCategoryService
    {
        Task<IActionResult> EquipmentCategoryCreateAsync(EquipmentCategoryRequest request);
        Task<IActionResult> EquipmentCategoryByIdAsync(int category_id);
        Task<IActionResult> EquipmentCategoryDeleteAsync(int category_id);
        Task<IActionResult> EquipmentCategoryListAsync(string? search, int length, int page, string orderColumn, string orderDirection);
    }
}
