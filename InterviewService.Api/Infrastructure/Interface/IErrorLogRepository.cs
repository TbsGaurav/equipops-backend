using InterviewService.Api.Helpers;

namespace InterviewService.Api.Infrastructure.Interface
{
    public interface IErrorLogRepository
    {
        Task LogAsync(ErrorLogModel log);
    }
}
