using EquipOps.Model.DowntimeLog;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Services.Interface
{
    public interface IDowntimeLogService
    {
        Task<IActionResult> DowntimeLogCreateUpdateAsync(DowntimeLogRequest request);
    }
}
