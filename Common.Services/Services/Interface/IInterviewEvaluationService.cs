using Common.Services.ViewModels.InterviewEvaluation;

namespace Common.Services.Services.Interface
{
    public interface IInterviewEvaluationService
    {
        Task<InterviewEvaluationResponse> EvaluateAsync(InterviewTranscriptRequest request);

    }
}
