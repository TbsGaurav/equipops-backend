using EquipOps.Model.AuditLog;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuditLogController(IAuditLogService auditLogService) : ControllerBase
    {
        [HttpPost("auditLogCreate")]
        public async Task<IActionResult> AuditLogCreate([FromBody] AuditLogRequest request)
        {
            var result = await auditLogService.AuditLogCreateAsync(request);
            return Ok(result);
        }

        [HttpGet("auditLogById")]
        public async Task<IActionResult> AuditLogById(int audit_id)
        {
            var result = await auditLogService.AuditLogByIdAsync(audit_id);
            return Ok(result);
        }

        [HttpGet("auditLogList")]
        public async Task<IActionResult> AuditLogList(string? search = "", string? entityName = null, string? action = null, int length = 10, int page = 1)
        {
            var result = await auditLogService.AuditLogListAsync(search, entityName, action, length, page);
            return Ok(result);
        }
    }
}
