namespace InterviewService.Api.ViewModels.Request.JobObjective
{
    public class JobObjectiveRequestViewModel
    {
        public string workMode { get; set; } = string.Empty;
        public string jobType { get; set; } = string.Empty;
        public int experienceYears { get; set; }
        public string objective { get; set; } = string.Empty;
    }
}
