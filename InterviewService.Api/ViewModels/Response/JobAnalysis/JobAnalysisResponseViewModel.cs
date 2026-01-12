namespace InterviewService.Api.ViewModels.Response.JobDetail
{
    public class JobAnalysisResponseViewModel
    {
        public JobInfo JobInfo { get; set; } = new JobInfo();
        public List<CandidateOverview> CandidateOverview { get; set; } = [];
        public JobKpis Kpis { get; set; } = new JobKpis();
        public List<PerformanceTrend> PerformanceTrend { get; set; } = [];
        public CandidateSentiment CandidateSentiment { get; set; } = new CandidateSentiment();
        public CandidateStatus CandidateStatus { get; set; } = new CandidateStatus();
        public List<CandidatePendingInterview> candidatePendingInterviews { get; set; } = [];
    }

    public class JobInfo
    {
        public Guid InterviewId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public int NoOfQuestions { get; set; }
    }
    public class CandidateOverview
    {
        public Guid CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public decimal OverallScore { get; set; }
        public decimal Communication { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
    public class JobKpis
    {
        public int CandidatesInterviewed { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal InterviewCompletionRate { get; set; }
        public string AverageDuration { get; set; } = "0m 0s";
    }
    public class PerformanceTrend
    {
        public string Month { get; set; } = string.Empty;
        public decimal AverageScore { get; set; }
    }
    public class CandidateSentiment
    {
        public int Positive { get; set; }
        public int Neutral { get; set; }
        public int Negative { get; set; }
    }
    public class CandidateStatus
    {
        public int Hired { get; set; }
        public int Rejected { get; set; }
        public int Shortlisted { get; set; }
        public int Active { get; set; }
    }
    public class CandidatePendingInterview
    {
        public string CandidateName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public string InterviewName { get; set; } = string.Empty;
    }
}
