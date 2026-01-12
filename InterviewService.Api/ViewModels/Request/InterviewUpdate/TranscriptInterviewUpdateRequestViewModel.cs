namespace InterviewService.Api.ViewModels.Request.InterviewUpdate
{
    public class TranscriptInterviewUpdateRequestViewModel
    {
        public Guid id { get; set; }
        public Guid interview_id { get; set; }
        public Guid candidate_id { get; set; }
        public decimal technical { get; set; }
        public decimal communication { get; set; }
        public decimal confidence { get; set; }
        public decimal overall { get; set; }
        public string hiring_decision { get; set; } = string.Empty;
        public string hiring_reason { get; set; } = string.Empty;
        public string transcript { get; set; } = string.Empty;
        public string questions_evaluation { get; set; } = string.Empty;
        public Guid created_by { get; set; }
    }
}
