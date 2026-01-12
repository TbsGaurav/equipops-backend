namespace InterviewService.Api.ViewModels.Response.Interviewer_setting
{
    public class InterviewerSettingCreateUpdateResponseViewModel
    {
        public Guid? Id { get; set; }
        public Guid? Interviewer_id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public bool Is_Active { get; set; } = true;
        public bool Is_Delete { get; set; }
        public Guid? Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public Guid? Updated_By { get; set; }
        public DateTime Updated_Date { get; set; }
    }
}
