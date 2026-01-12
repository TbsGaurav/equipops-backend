using InterviewService.Api.ViewModels.Request.Interviewer;
using InterviewService.Api.ViewModels.Response.Interviewer;

namespace InterviewService.Api.Infrastructure.Interface
{
    public interface IInterviewerRepository
    {
        Task<List<InterviewerData>> GetInterviewersAsync(Guid? organizationId);
        Task<InterviewerListResponseViewModel> GetInterviewerByIdAsync(Guid? Id);
        Task<InterviewerCreateUpdateResponseViewModel> CreateInterviewerAsync(InterviewerDataCreateRequestViewModel request);
        Task<InterviewerCreateUpdateResponseViewModel> UpdateInterviewerAsync(InterviewerDataUpdateRequestViewModel request);
        Task DeleteInterviewerAsync(InterviewerDeleteRequestViewModel request);
        Task<string?> Get_retell_LLM_key(Guid organizationId);
        Task<string?> GetRetellAiKey(Guid organizationId);
        Task<RetellAiVoiceModel?> GetVoiceById(string voiceId, string RetellApiKey);
        Task<string?> GetOpenAiKey(Guid organizationId);
    }
}
