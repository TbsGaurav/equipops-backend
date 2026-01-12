using Microsoft.AspNetCore.Mvc;

using OrganizationService.Api.Helpers.ResponseHelpers.Enums;
using OrganizationService.Api.Services.Interface;
using OrganizationService.Api.ViewModels.Request.Candidate;

namespace OrganizationService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateService _candidateService;

        public CandidateController(ICandidateService candidateService)
        {
            _candidateService = candidateService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetCandidates(string? Search, int Length = 10, int Page = 1, string OrderColumn = "name", string OrderDirection = "Asc", bool? IsActive = null)
        {
            // 🔹 Call Service
            var result = await _candidateService.GetCandidatesAsync(Search, Length, Page, OrderColumn, OrderDirection, IsActive);
            return result;
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetCandidateById(Guid Id)
        {
            // 🔹 Call Service
            var result = await _candidateService.GetCandidateByIdAsync(Id);
            return result;
        }

        [HttpPost("create-update")]
        public async Task<IActionResult> CreateUpdateCandidate([FromForm] CandidateCreateUpdateRequestViewModel request)
        {
            // 🔹 Call Service
            request.json_form_data.CandidateUploadType = Convert.ToInt32(CandidateUploadType.Candidate_Manual);
            var result = await _candidateService.CreateUpdateCandidateAsync(request, false);
            return result;
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteCandidate([FromBody] CandidateDeleteRequestViewModel request)
        {
            // 🔹 Call Service
            var result = await _candidateService.DeleteCandidateAsync(request);
            return result;
        }
        [HttpPost("Candidate_Interview_Invitation")]
        public async Task<IActionResult> CandidateInterviewInvitation([FromForm] CanidateDirectInvitationDetail request)
        {
            // 🔹 Call Service
            CandidateCreateUpdateRequestViewModel request1 = new CandidateCreateUpdateRequestViewModel();

            request1.json_form_data = new CanidateDetail(); // ✅ REQUIRED
            request1.json_form_data.Resume = request.Resume;

            request1.json_form_data.InterviewId = request.InterviewId;
            request1.json_form_data.Name = request.Name;
            request1.json_form_data.Email = request.Email;
            request1.json_form_data.CandidateUploadType = Convert.ToInt32(CandidateUploadType.User_Manual);
            var result = await _candidateService.CreateUpdateCandidateAsync(request1, true);
            return result;
        }
        [HttpPost("CanidateResumeBulk")]
        public async Task<IActionResult> CanidateResumeBulk([FromForm] CanidateResumeList request)
        {
            request.CandidateUploadType = Convert.ToInt32(CandidateUploadType.User_Multi_Candidate);
            // 🔹 Call Service
            var result = await _candidateService.CanidateResumeBulkAsync(request.ResumeDetail);
            return result;
        }
    }
}
