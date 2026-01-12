using Common.Services.Services.Interface;
using Common.Services.ViewModels.MatchMatching;
using Common.Services.ViewModels.ResumeParse;


namespace InterviewService.Api.Services.Implementation
{
    public class MatchMachingService : IMatchMachingService
    {
        public float CalculateMatchScore(JobRequirementViewModel job, ResumeProfile candidate)
        {
            var hardSkill = HardSkillMatch(job, candidate) ?? 0;
            var experience = ExperienceMatch(job, candidate) ?? 0;
            //var education = EducationMatch(job, candidate) ?? 0;
            //var salary = SalaryMatch(job, candidate) ?? 0;

            return
                hardSkill * MatchWeights.HardSkill +
                experience * MatchWeights.Experience;
            //education * MatchWeights.Education +
            //salary * MatchWeights.Salary;
        }

        #region private method
        //Hard Skill Match
        private static float? HardSkillMatch(JobRequirementViewModel job, ResumeProfile candidate)
        {
            if (!job.RequiredSkills.Any()) return 100;
            if ((bool)!candidate?.Skills.Any()) return 0;

            int matched = job.RequiredSkills
                .Count(s => candidate.Skills
                    .Any(c => c.Equals(s, StringComparison.OrdinalIgnoreCase)));

            return matched * 100f / job.RequiredSkills.Count;
        }

        //Experience Match
        private static float? ExperienceMatch(JobRequirementViewModel job, ResumeProfile candidate)
        {
            if (candidate?.TotalExperienceYears >= job.MinExperienceYears)
                return 100;

            return (float?)(candidate?.TotalExperienceYears * 100f / job.MinExperienceYears);
        }

        // Education Match
        private static float? EducationMatch(JobRequirementViewModel job, ResumeProfile candidate)
        {
            if (string.IsNullOrWhiteSpace(job.RequiredEducation))
                return 100;

            return candidate.Education.Any(e =>
                e.Contains(job.RequiredEducation, StringComparison.OrdinalIgnoreCase))
                ? 100
                : 0;
        }

        //Salary Match
        private static float? SalaryMatch(JobRequirementViewModel job, ResumeProfile candidate)
        {
            if (Convert.ToDecimal(candidate.ExpectedSalary) <= job.SalaryBudget)
                return 100;

            return (float)(job.SalaryBudget / Convert.ToDecimal(candidate.ExpectedSalary) * 100);
        }
        #endregion
    }
}
