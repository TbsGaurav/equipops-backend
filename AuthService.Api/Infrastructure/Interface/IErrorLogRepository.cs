using AuthService.Api.Helpers;

namespace AuthService.Api.Infrastructure.Interface
{
    public interface IErrorLogRepository
    {
        Task LogAsync(ErrorLogModel log);
    }
}
