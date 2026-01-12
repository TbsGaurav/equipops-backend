using OrganizationService.Api.ViewModels.Request.Candidate;
using OrganizationService.Api.ViewModels.Response.Candidate;

using System.Text.Json;

namespace OrganizationService.Api.Infrastructure.Interface
{
    public interface ICandidateRepository
    {
        Task<CandidateListResponseViewModel> GetCandidatesAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null);
        Task<CandidateByIdResponseViewModel> GetCandidateByIdAsync(Guid Id);
        Task<CandidateCreateUpdateResponseViewModel> CreateUpdateCandidateAsync(CandidateCreateUpdateRequestViewModel request, string Resume_url, string TotalExperience, string Skill, JsonElement json, decimal matchScore);
        Task DeleteCandidateAsync(CandidateDeleteRequestViewModel request);
        Task<string?> GetOpenAiKey(Guid organizationId, Guid interviewId);
        Task<List<string>?> GetSkillsAsync(Guid interviewId);
        Task<string> CreateInterviewTokenAsync(InterviewTokenRequestViewModel Interview_Id, bool IsDirect);
    }
}
