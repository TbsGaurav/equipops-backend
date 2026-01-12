using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interview_transcript;

using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewTranscriptController(IInterviewTranscriptService interviewTranscript) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateInterviewTranscript([FromBody] InterviewTrasncriptCreateRequestViewModel request)
        {
            var response = await interviewTranscript.CreateInterviewTranscriptAsync(request);
            return response;
        }
    }
}
