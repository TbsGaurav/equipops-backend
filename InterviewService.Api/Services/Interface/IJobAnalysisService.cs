using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Api.Services.Interface
{
    public interface IJobAnalysisService
    {
        Task<IActionResult> GetJobAnalysisAsync(Guid id);
        Task<IActionResult> GetCandidateJobAnalysis(Guid id);
    }
}
