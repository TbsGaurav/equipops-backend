using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interview_Detail;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewDetailController : Controller
    {
        private readonly IInterviewDetailService _interviewDetailService;
        private readonly ILogger<InterviewDetailController> _iLogger;
        public InterviewDetailController(IInterviewDetailService interviewDetailService, ILogger<InterviewDetailController> logger)
        {
            _interviewDetailService = interviewDetailService;
            _iLogger = logger;
        }

        #region Interview Detail
        [HttpPost("interview_Complete")]
        public async Task<IActionResult> interview_Complete([FromBody] InterviewDetailRequestViewModel request)
        {
            var result = await _interviewDetailService.InterviewDetailCreateAsync(request);
            return Ok(result);
        }
        [AllowAnonymous]
        [HttpGet("get_Interview_Detail")]
        public async Task<IActionResult> GetInterviewDetail(Guid? interview_id, Guid? candidate_id)
        {
            var result = await _interviewDetailService.InterviewDetailAsync(interview_id, candidate_id);
            return Ok(result);
        }
        #endregion
    }
}
