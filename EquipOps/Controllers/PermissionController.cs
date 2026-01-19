using EquipOps.Model.Permission;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class PermissionController(IPermissionService permissionService) : ControllerBase
    {
        [HttpPost("permissionCreateUpdate")]
        public async Task<IActionResult> PermissionCreateUpdate([FromBody] PermissionRequest request)
        {
            var result = await permissionService.PermissionCreateUpdateAsync(request);
            return Ok(result);
        }

        [HttpGet("permissionById")]
        public async Task<IActionResult> PermissionById(int permission_id)
        {
            var result = await permissionService.PermissionByIdAsync(permission_id);
            return Ok(result);
        }

        [HttpGet("permissionList")]
        public async Task<IActionResult> PermissionList(string? search = "",bool? status = null,int length = 10,int page = 1,string orderColumn = "permission_code",string orderDirection = "ASC")
        {
            return await permissionService.PermissionListAsync(
                search, status, length, page, orderColumn, orderDirection
            );
        }


        [HttpPost("permissionDelete")]
        public async Task<IActionResult> PermissionDelete([FromBody] PermissionDeleteRequestViewModel request)
        {
            int permission_id = request.permission_id;
            var result = await permissionService.PermissionDeleteAsync(permission_id);
            return Ok(result);
        }
    }
}
