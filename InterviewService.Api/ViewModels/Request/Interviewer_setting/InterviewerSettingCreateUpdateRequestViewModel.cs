namespace InterviewService.Api.ViewModels.Request.Interviewer_setting
{
    public class InterviewerSettingCreateUpdateRequestViewModel
    {
        public Guid? Id { get; set; }
        public Guid? Interviewer_id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }

    }
}
