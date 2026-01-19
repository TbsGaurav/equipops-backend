using EquipOps.Model.AuditLog;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Services.Interface
{
    public interface IAuditLogService
    {
        Task<IActionResult> AuditLogCreateAsync(AuditLogRequest request);
        Task<IActionResult> AuditLogByIdAsync(long auditId);
        Task<IActionResult> AuditLogListAsync(string? search, string? entityName, string? action, int length, int page);
    }
}
