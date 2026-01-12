using Common.Services.ViewModels.MatchMatching;
using Common.Services.ViewModels.ResumeParse;

namespace Common.Services.Services.Interface
{
    public interface IMatchMachingService
    {
        float CalculateMatchScore(JobRequirementViewModel job, ResumeProfile candidate);
    }
}
