namespace Common.Services.ViewModels.MatchMatching
{
    public class JobRequirementViewModel
    {
        public List<string> RequiredSkills { get; set; } = new();
        public Dictionary<string, int> SoftSkills { get; set; } = new();
        public float? MinExperienceYears { get; set; }
        public string? RequiredEducation { get; set; } = "";
        public decimal? SalaryBudget { get; set; }
    }
}
