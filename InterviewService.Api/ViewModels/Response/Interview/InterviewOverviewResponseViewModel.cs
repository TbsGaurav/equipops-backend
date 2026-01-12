namespace InterviewService.Api.ViewModels.Response.Interview
{
    public class InterviewOverviewResponseViewModel
    {
        public CandidateInfo Candidate { get; set; } = new CandidateInfo();
        public InterviewInfo Interview { get; set; } = new InterviewInfo();
        public InterviewerInfo Interviewer { get; set; } = new InterviewerInfo();
    }
    public class CandidateInfo
    {
        public Guid Candidate_id { get; set; }
        public string Candidate_name { get; set; } = string.Empty;
        public string Candidate_avatar { get; set; } = string.Empty;
    }

    public class InterviewInfo
    {
        public Guid Interview_id { get; set; }
        public string Interview_name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Duration_mins { get; set; } = string.Empty;
    }

    public class InterviewerInfo
    {
        public Guid Interviewer_id { get; set; }
        public string Interviewer_name { get; set; } = string.Empty;
        public string Interviewer_avatar { get; set; } = string.Empty;
    }
}
