using Microsoft.AspNetCore.Mvc;

using SettingService.Api.Services.Interface;
using SettingService.Api.ViewModels.Request.Menu_type;

namespace SettingService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuTypeController(ILogger<MenuTypeController> logger, IMenuTypeService menuTypeService) : ControllerBase
    {
        #region Menu Type
        [HttpPost("createUpdate")]
        public async Task<IActionResult> CreateOrUpdateMenuType(MenuTypeCreateUpdateRequestViewModel request)
        {
            var result = await menuTypeService.CreateUpdateMenuTypeAsync(request);
            return result;

        }
        [HttpGet("list")]
        public async Task<IActionResult> GetMenuTypeList(string? search, int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "Asc", bool? isActive = null)
        {
            var result = await menuTypeService.GetMenuTypeListAsync(search, length, page, orderColumn, orderDirection, isActive);
            return result;
        }
        [HttpGet("getById")]
        public async Task<IActionResult> GetMenuTypeById(Guid? Id)
        {
            var result = await menuTypeService.GetMenuTypeByIdAsync(Id);
            return result;
        }
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteMenuType([FromBody] MenuTypeDeleteRequestViewModel request)
        {
            var result = await menuTypeService.DeleteMenuTypeAsync(request);
            return result;
        }
        #endregion

        #region Menu Permission
        [HttpPost("menuPermissionCreate")]
        public async Task<IActionResult> menuPermissionCreate([FromBody] MenuPermissionCreateUpdateRequestViewModel request)
        {
            var result = await menuTypeService.CreateUpdateMenuPermissionAsync(request);
            return Ok(result);
        }

        [HttpGet("menuPermissionList")]
        public async Task<IActionResult> GetMenuPermissionList(string? search = "", bool? Is_Active = null, int length = 10, int page = 1, string orderColumn = "slug", string orderDirection = "ASC")
        {
            var result = await menuTypeService.MenuPermissionListAsync(search, Is_Active, length, page, orderColumn, orderDirection);
            return Ok(result);
        }

        [HttpGet("menuPermissionById")]
        public async Task<IActionResult> GetMenuPermissionById(Guid? id)
        {
            var result = await menuTypeService.MenuPermissionByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("menuPermissionDelete")]
        public async Task<IActionResult> menuPermissionDelete([FromBody] MenuPermissionDeleteRequestViewModel request)
        {
            var result = await menuTypeService.MenuPermissionDeleteAsync(request);
            return Ok(result);
        }
        #endregion
    }
}
