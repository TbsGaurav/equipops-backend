using InterviewService.Api.ViewModels.Request.Interview_transcript;
using InterviewService.Api.ViewModels.Request.InterviewUpdate;

namespace InterviewService.Api.Infrastructure.Interface
{
    public interface IInterviewTranscriptRepository
    {
        Task<string> CreateInterviewTranscriptAsync(InterviewTrasncriptCreateRequestViewModel model, string ConversationText);
        Task<string> UpdateInterview(TranscriptInterviewUpdateRequestViewModel model);
    }
}
