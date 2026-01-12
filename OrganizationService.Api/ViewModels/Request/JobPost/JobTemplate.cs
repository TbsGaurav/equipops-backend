namespace OrganizationService.Api.ViewModels.Request.JobPost
{
    public class JobTemplate
    {
        public Guid? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SubTitle { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public Guid? EmploymentTypeId { get; set; }
        public int? ExperienceMin { get; set; }
        public int? ExperienceMax { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string Skills { get; set; } = string.Empty;
        public string[] Responsibilities { get; set; } = Array.Empty<string>();
        public string[] Requirements { get; set; } = Array.Empty<string>();
        public string Benefits { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
