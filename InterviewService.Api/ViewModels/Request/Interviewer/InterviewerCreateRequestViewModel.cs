namespace InterviewService.Api.ViewModels.Request.Interviewer
{
    public class InterviewerCreateRequestViewModel
    {
        public string Voice_id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public IFormFile? Avatar { get; set; }
    }
}
