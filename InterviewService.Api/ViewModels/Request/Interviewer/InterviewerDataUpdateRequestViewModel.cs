namespace InterviewService.Api.ViewModels.Request.Interviewer
{
    public class InterviewerDataUpdateRequestViewModel
    {
        public Guid? Id { get; set; }
        public string Voice_id { get; set; } = string.Empty;
        public string Agent_id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Avatar_url { get; set; } = string.Empty;
        public string Record_url { get; set; } = string.Empty;
    }
}
