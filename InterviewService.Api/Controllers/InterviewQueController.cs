using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interview_Que;

using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewQueController : Controller
    {
        private readonly IInterviewQueService _interview_QueService;
        private readonly ILogger<InterviewQueController> _iLogger;
        public InterviewQueController(IInterviewQueService interview_QueService, ILogger<InterviewQueController> logger)
        {
            _interview_QueService = interview_QueService;
            _iLogger = logger;
        }

        #region Interview Que
        [HttpPost("interview_QueCreate")]
        public async Task<IActionResult> interview_QueCreate([FromBody] InterviewQueRequestViewModel request)
        {
            var result = await _interview_QueService.InterviewQueCreateAsync(request);
            return Ok(result);
        }

        [HttpGet("interview_QueList")]
        public async Task<IActionResult> GetInterview_QueList(string? search = "", bool? Is_Active = null, int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "ASC")
        {
            var result = await _interview_QueService.InterviewQueListAsync(search, Is_Active, length, page, orderColumn, orderDirection);
            return Ok(result);
        }
        [HttpGet("interview_QueById")]
        public async Task<IActionResult> GetInterview_QueById(Guid? id)
        {
            var result = await _interview_QueService.InterviewQueByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("interview_QueDelete")]
        public async Task<IActionResult> interview_QueDelete([FromBody] InterviewQueDeleteRequestViewModel request)
        {
            var result = await _interview_QueService.InterviewQueDeleteAsync(request);
            return Ok(result);
        }
        #endregion
    }
}
