using EquipOps.Model.Role;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class RoleController(IRoleService roleService) : ControllerBase
    {
        [HttpPost("roleCreateUpdate")]
        public async Task<IActionResult> RoleCreateUpdate([FromBody] RoleRequest request)
        {
            return await roleService.RoleCreateUpdateAsync(request);
        }

        [HttpGet("roleList")]
        public async Task<IActionResult> RoleList(string? search = "",bool? is_active = null,int length = 10,int page = 1,string orderColumn = "created_date",string orderDirection = "DESC")
        {
            return await roleService.RoleListAsync(search, is_active, length, page, orderColumn, orderDirection);
        }

        [HttpGet("roleById")]
        public async Task<IActionResult> RoleById(int role_id)
        {
            return await roleService.RoleByIdAsync(role_id);
        }

        [HttpPost("roleDelete")]
        public async Task<IActionResult> RoleDelete([FromBody] RoleDeleteRequestViewModel request)
        {
            return await roleService.RoleDeleteAsync(request.role_id);
        }
    }
}
