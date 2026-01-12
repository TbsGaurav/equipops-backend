using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interviewer;

using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewerController : ControllerBase
    {
        private readonly ILogger<InterviewerController> _iLogger;
        private readonly IInterviewerService _interviewerService;

        public InterviewerController(ILogger<InterviewerController> logger, IInterviewerService interviewerService)
        {
            _iLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            _interviewerService = interviewerService ?? throw new ArgumentNullException(nameof(interviewerService));
        }
        [HttpGet("list")]
        public async Task<IActionResult> GetInterviewers(Guid? organizationId)
        {
            // 🔹 Call Service
            var result = await _interviewerService.GetInterviewersAsync(organizationId);
            return result;
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetInterviewerById(Guid Id)
        {
            // 🔹 Call Service
            var result = await _interviewerService.GetInterviewerByIdAsync(Id);
            return result;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateInterviewer([FromForm] InterviewerCreateRequestViewModel request)
        {
            // 🔹 Call Service
            var result = await _interviewerService.CreateInterviewerAsync(request);
            return result;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateInterviewer([FromForm] InterviewerUpdateRequestViewModel request)
        {
            // 🔹 Call Service
            var result = await _interviewerService.UpdateInterviewerAsync(request);
            return result;
        }


        [HttpPost("delete")]
        public async Task<IActionResult> DeleteInterviewer([FromBody] InterviewerDeleteRequestViewModel request)
        {
            var result = await _interviewerService.DeleteInterviewerAsync(request);
            return result;
        }
        [HttpGet("voices")]
        public async Task<IActionResult> GetVoices()
        {
            var voices = await _interviewerService.GetVoices();
            return voices;
        }

        [HttpGet("voice-by-id")]
        public async Task<IActionResult> GetVoiceById(string VoiceId)
        {
            var voice = await _interviewerService.GetVoiceById(VoiceId);
            return voice;
        }
    }
}
