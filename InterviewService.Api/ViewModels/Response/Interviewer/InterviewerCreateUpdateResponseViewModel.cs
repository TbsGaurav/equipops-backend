namespace InterviewService.Api.ViewModels.Response.Interviewer
{
    public class InterviewerCreateUpdateResponseViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Agent_id { get; set; } = string.Empty;
        public string Voice_id { get; set; } = string.Empty;
        public string Avatar_url { get; set; } = string.Empty;
        public string Record_url { get; set; } = string.Empty;
        public Guid? Organization_id { get; set; }
        public bool Is_Active { get; set; } = true;
        public bool Is_Delete { get; set; }
        public Guid? Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public Guid? Updated_By { get; set; }
        public DateTime Updated_Date { get; set; }
    }
}
