using SettingService.Api.ViewModels.Response;

namespace InterviewService.Api.ViewModels.Response.Interview_Detail
{
    public class InterviewDetailResponseViewModel
    {
        public List<InterviewDetail> interviewDetail { get; set; } = new List<InterviewDetail>();
        public List<InterviewTranscript> interviewtranscript { get; set; } = new List<InterviewTranscript>();
    }

    public class InterviewCompleteResponseViewModel
    {

    }

    public class InterviewTranscript : CommonParameterAllList
    {
        public Guid id { get; set; }
        public Guid interview_id { get; set; }
        public Guid candidate_id { get; set; }
        public Guid interviewer_id { get; set; }
        public Guid interview_update_id { get; set; }
        public string? interviewer_text { get; set; }
        public string? candidate_text { get; set; }
    }
    public class InterviewDetail : CommonParameterAllList
    {
        public Guid id { get; set; }
        public Guid interview_id { get; set; }
        public Guid candidate_id { get; set; }
        public string? name { get; set; }
        public DateTime interview_date { get; set; }
        public decimal overall_score { get; set; }
        public decimal communication_score { get; set; }
        public string? description { get; set; }
        public string? record_url { get; set; }
    }
}
