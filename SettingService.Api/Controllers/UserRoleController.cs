using Microsoft.AspNetCore.Mvc;

using SettingService.Api.Services.Interface;
using SettingService.Api.ViewModels.Request;

namespace SettingService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;
        private readonly ILogger<UserRoleController> _iLogger;
        public UserRoleController(IUserRoleService userRoleService, ILogger<UserRoleController> logger)
        {
            _userRoleService = userRoleService;
            _iLogger = logger;
        }
        #region User Role
        [HttpPost("userRoleCreate")]
        public async Task<IActionResult> userRoleCreate([FromBody] UserRoleRequestViewModel request)
        {
            var result = await _userRoleService.UserRoleCreateAsync(request);
            return Ok(result);
        }

        [HttpGet("userRoleList")]
        public async Task<IActionResult> GetUserRoleList(string? search = "", bool? Is_Active = null, int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "ASC")
        {
            var result = await _userRoleService.UserRoleListAsync(search, Is_Active, length, page, orderColumn, orderDirection);
            return Ok(result);
        }
        [HttpGet("userRoleById")]
        public async Task<IActionResult> GetUserRoleById(Guid? id)
        {
            var result = await _userRoleService.UserRoleByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("userRoleDelete")]
        public async Task<IActionResult> userRoleDelete([FromBody] UserRoleDeleteRequestViewModel request)
        {
            var result = await _userRoleService.UserRoleDeleteAsync(request);
            return Ok(result);
        }
        #endregion

        #region User Access Role
        [HttpPost("userAccessRoleCreate")]
        public async Task<IActionResult> userAccessRoleCreate([FromBody] UserAccessRoleRequestViewModel request)
        {
            var result = await _userRoleService.UserAccessRoleCreateAsync(request);
            return Ok(result);
        }

        [HttpGet("userAccessRoleList")]
        public async Task<IActionResult> GetUserAccessRoleList(string? search = "", bool? Is_Active = null, int length = 10, int page = 1, string orderColumn = "menu_name", string orderDirection = "ASC")
        {
            var result = await _userRoleService.UserAccessRoleListAsync(search, Is_Active, length, page, orderColumn, orderDirection);
            return Ok(result);
        }
        [HttpGet("userAccessRoleById")]
        public async Task<IActionResult> GetUserAccessRoleById(Guid? id)
        {
            var result = await _userRoleService.UserAccessRoleByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("userAccessRoleDelete")]
        public async Task<IActionResult> userAccessRoleDelete([FromBody] UserAccessRoleDeleteRequestViewModel request)
        {
            var result = await _userRoleService.UserAccessRoleDeleteAsync(request);
            return Ok(result);
        }
        #endregion
    }
}
