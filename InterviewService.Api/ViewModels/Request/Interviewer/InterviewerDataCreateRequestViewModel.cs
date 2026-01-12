namespace InterviewService.Api.ViewModels.Request.Interviewer
{
    public class InterviewerDataCreateRequestViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Agent_id { get; set; } = string.Empty;
        public string Voice_id { get; set; } = string.Empty;
        public string Avatar_url { get; set; } = string.Empty;
        public string Record_url { get; set; } = string.Empty;
        public Guid? Organization_id { get; set; }
    }
}
