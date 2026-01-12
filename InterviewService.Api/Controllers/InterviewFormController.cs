using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interview_Form;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewFormController : Controller
    {
        private readonly IInterviewFormService _interview_FormService;
        private readonly ILogger<InterviewFormController> _iLogger;
        public InterviewFormController(IInterviewFormService interview_FormService, ILogger<InterviewFormController> logger)
        {
            _interview_FormService = interview_FormService;
            _iLogger = logger;
        }

        #region Interview Form
        [HttpPost("interview_FormCreate")]
        public async Task<IActionResult> interview_FormCreate([FromBody] InterviewFormRequestViewModel request)
        {
            var result = await _interview_FormService.InterviewFormCreateAsync(request);
            return Ok(result);
        }

        [HttpGet("interview_FormList")]
        public async Task<IActionResult> GetInterview_FormList(string? search = "", bool? Is_Active = null, int length = 10, int page = 1, string orderColumn = "created_date", string orderDirection = "ASC")
        {
            var result = await _interview_FormService.InterviewFormListAsync(search, Is_Active, length, page, orderColumn, orderDirection);
            return Ok(result);
        }
        [HttpGet("interview_FormById")]
        public async Task<IActionResult> GetInterview_FormById(Guid? id)
        {
            var result = await _interview_FormService.InterviewFormByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("interview_FormDelete")]
        public async Task<IActionResult> interview_FormDelete([FromBody] InterviewFormDeleteRequestViewModel request)
        {
            var result = await _interview_FormService.InterviewFormDeleteAsync(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("interview_FormByInterviewFormId")]
        public async Task<IActionResult> GetInterview_FormByInterviewId(Guid? id)
        {
            var result = await _interview_FormService.InterviewFormByIdAsync(id);
            return Ok(result);
        }
        #endregion
    }
}
