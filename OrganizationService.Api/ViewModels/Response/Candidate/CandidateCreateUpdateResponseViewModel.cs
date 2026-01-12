namespace OrganizationService.Api.ViewModels.Response.Candidate
{
    public class CandidateCreateUpdateResponseViewModel
    {
        public Guid Id { get; set; }
        public Guid InterviewId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Experience { get; set; }
        public string? Skill { get; set; }
        public string? Description { get; set; }
        public string? ResumeUrl { get; set; }
        public string? Email { get; set; }
        public string? Phone_Number { get; set; }
        public float TotalScore { get; set; }
        public string? TotalExperience { get; set; }
    }
}
