using OrganizationService.Api.Helpers;

namespace OrganizationService.Api.Infrastructure.Interface
{
    public interface IErrorLogRepository
    {
        Task LogAsync(ErrorLogModel log);
    }
}
