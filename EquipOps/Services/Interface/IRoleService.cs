using EquipOps.Model.Role;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Services.Interface
{
    public interface IRoleService
    {
        Task<IActionResult> RoleCreateUpdateAsync(RoleRequest request);
        Task<IActionResult> RoleListAsync(string? search, bool? is_active, int length, int page, string orderColumn, string orderDirection);
        Task<IActionResult> RoleByIdAsync(int role_id);
        Task<IActionResult> RoleDeleteAsync(int role_id);
    }
}
