using InterviewService.Api.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobAnalysisController : ControllerBase
    {
        private readonly IJobAnalysisService _jobAnalysisService;
        private readonly ILogger<JobAnalysisController> _logger;

        public JobAnalysisController(IJobAnalysisService jobAnalysisService, ILogger<JobAnalysisController> logger)
        {
            _jobAnalysisService = jobAnalysisService;
            _logger = logger;
        }

        [HttpGet("job-analysis")]
        public async Task<IActionResult> GetJobAnalysis(Guid interviewId)
        {
            _logger.LogInformation("API Hit: GetJobAnalysis | InterviewId: {InterviewId}", interviewId);

            var result = await _jobAnalysisService.GetJobAnalysisAsync(interviewId);

            return result;
        }

        [HttpGet("candidate-job-analysis")]
        public async Task<IActionResult> GetCandidateJobAnalysis(Guid candidateId)
        {
            _logger.LogInformation("API Hit: GetCandidateJobAnalysis | candidateId: {candidateId}", candidateId);

            var result = await _jobAnalysisService.GetCandidateJobAnalysis(candidateId);

            return result;
        }
    }
}
