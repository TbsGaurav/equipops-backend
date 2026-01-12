namespace InterviewService.Api.ViewModels.Request.Interview_transcript
{
    public class InterviewTrasncriptCreateRequestViewModel
    {
        public string Call_id { get; set; } = string.Empty;
        public Guid? Candidate_id { get; set; }
        public Guid? Interview_id { get; set; }
        public Guid? interview_update_id { get; set; }
        public Guid? Interviewer_id { get; set; }
        public Guid Organization_id { get; set; }
    }
}
