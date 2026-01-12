using Common.Services.ViewModels.ResumeParse;

namespace Common.Services.Services.Interface
{
    public interface IResumeParseService
    {
        Task<ResumeParseResult> ParseResumeAsync(string resumeText, string apiKey);
        Task<List<ResumeParseResult>> ParseResumesBatchAsync(List<string> resumes, string azureApiKey);
    }
}
