namespace InterviewService.Api.ViewModels.Response.Interview
{
    public class InterviewTypeListResponseViewModel
    {
        public int TotalNumbers { get; set; }
        public List<InterviewTypeData> InterviewTypeData { get; set; } = new List<InterviewTypeData>();
    }

    public class InterviewTypeData
    {
        public Guid Id { get; set; }
        public string Interview_Type { get; set; } = string.Empty;
        public bool Is_Active { get; set; }
        public bool Is_Delete { get; set; }
        public Guid? Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public Guid? Updated_By { get; set; }
        public DateTime Updated_Date { get; set; }
    }
}