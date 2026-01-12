using InterviewService.Api.ViewModels.Response.JobDetail;

namespace InterviewService.Api.Infrastructure.Interface
{
    public interface IJobAnalysisRepository
    {
        Task<JobAnalysisResponseViewModel> GetJobAnalysisAsync(Guid id);
        Task<CandidateJobAnalysisResponseViewModel> GetCandidateJobAnalysis(Guid id);
    }
}
