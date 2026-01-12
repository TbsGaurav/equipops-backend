namespace InterviewService.Api.ViewModels.Request.Interview_Type
{
    public class InterviewTypeCreateUpdateRequestViewModel
    {
        public Guid? Id { get; set; }
        public string Interview_Type { get; set; } = string.Empty;
    }
}
