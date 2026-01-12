namespace InterviewService.Api.ViewModels.Response.Interviewer
{
    public class InterviewerListResponseViewModel
    {
        public Guid? Id { get; set; }
        public string Agent_id { get; set; } = string.Empty;
        public string Voice_id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Avatar_url { get; set; } = string.Empty;
        public string Record_url { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public bool Is_Active { get; set; }
        public bool Is_Delete { get; set; }
        public Guid? Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public Guid? Updated_By { get; set; }
        public DateTime Updated_Date { get; set; }
        public RetellAiVoiceModel Voice_data { get; set; } = new RetellAiVoiceModel();
    }
    public class InterviewerData
    {
        public Guid? Id { get; set; }
        public string Agent_id { get; set; } = string.Empty;
        public string Voice_id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Avatar_url { get; set; } = string.Empty;
        public string Record_url { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public bool Is_Active { get; set; }
        public bool Is_Delete { get; set; }
        public Guid? Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public Guid? Updated_By { get; set; }
        public DateTime Updated_Date { get; set; }

    }
}
