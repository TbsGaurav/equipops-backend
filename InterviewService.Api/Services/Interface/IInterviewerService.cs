using InterviewService.Api.ViewModels.Request.Interviewer;

using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Api.Services.Interface
{
    public interface IInterviewerService
    {
        Task<IActionResult> GetInterviewersAsync(Guid? organizationId);
        Task<IActionResult> GetInterviewerByIdAsync(Guid Id);
        Task<IActionResult> CreateInterviewerAsync(InterviewerCreateRequestViewModel model);
        Task<IActionResult> UpdateInterviewerAsync(InterviewerUpdateRequestViewModel model);
        Task<IActionResult> DeleteInterviewerAsync(InterviewerDeleteRequestViewModel model);
        Task<IActionResult> GetVoices();
        Task<IActionResult> GetVoiceById(string VoiceId);
    }
}
