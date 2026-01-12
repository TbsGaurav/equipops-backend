namespace InterviewService.Api.ViewModels.Request.Interview_Detail
{
    public class InterviewDetailRequestViewModel
    {
        public Guid interview_id { get; set; }
        public Guid candidate_id { get; set; }
        public string? name { get; set; }
        public DateTime interview_date { get; set; }
        public decimal overall_score { get; set; }
        public decimal communication_score { get; set; }
        public string? description { get; set; }
        public string? record_url { get; set; }

        public List<InterviewTranscriptViewModel> interview_transcript { get; set; }

    }
    public class InterviewTranscriptViewModel
    {
        public Guid interviewer_id { get; set; }
        public Guid interview_update_id { get; set; }
        public string? interviewer_text { get; set; }
        public string? candidate_text { get; set; }
    }
}