using Common.Services.ViewModels.General;

namespace Common.Services.Services.Interface
{
    public interface IGeneralAIService
    {
        Task<IndustryDepartmentResponse> GetDepartmensByIndustryAsync(string industry, string apiKey);
        Task<JobObjectiveResponse> GetJobObjectivesAsync(string apiKey, string jobType, string workMode, int experienceYears, string objective);
    }
}
