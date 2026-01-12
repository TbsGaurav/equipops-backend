using SettingService.Api.Helpers;

namespace SettingService.Api.Infrastructure.Interface
{
    public interface IErrorLogRepository
    {
        Task LogAsync(ErrorLogModel log);
    }
}
