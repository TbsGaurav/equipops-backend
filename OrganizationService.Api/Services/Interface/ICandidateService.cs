using Common.Services.ViewModels.ResumeParse;

using Microsoft.AspNetCore.Mvc;

using OrganizationService.Api.ViewModels.Request.Candidate;

namespace OrganizationService.Api.Services.Interface
{
    public interface ICandidateService
    {
        Task<IActionResult> GetCandidatesAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null);
        Task<IActionResult> GetCandidateByIdAsync(Guid Id);
        Task<IActionResult> CreateUpdateCandidateAsync(CandidateCreateUpdateRequestViewModel model, bool IsDirect);
        Task<IActionResult> DeleteCandidateAsync(CandidateDeleteRequestViewModel model);
        Task<IActionResult> CanidateResumeBulkAsync(List<IFormFile> files);

        Task<MatchMatchingResponse> ParseResumeAsync(IFormFile file, Guid interviewId);
        Task<List<MatchMatchingResponse>> ParseResumeAsyncBatch(List<IFormFile> files, Guid interviewId);
    }
}
