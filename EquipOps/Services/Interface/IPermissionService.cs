using EquipOps.Model.Permission;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Services.Interface
{
    public interface IPermissionService
    {
        Task<dynamic> PermissionCreateUpdateAsync(PermissionRequest request);
        Task<dynamic> PermissionDeleteAsync(int permission_id);
        Task<dynamic> PermissionByIdAsync(int permission_id);
        Task<IActionResult> PermissionListAsync(string? search, bool? status, int length, int page, string orderColumn, string orderDirection);
    }
}
