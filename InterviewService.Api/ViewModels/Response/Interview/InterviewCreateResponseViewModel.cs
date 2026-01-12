namespace InterviewService.Api.ViewModels.Response.Interview
{
    public class InterviewCreateResponseViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? Interviewer_Id { get; set; }
        public Guid? Interview_Type_Id { get; set; }
        public Guid? Work_Mode_Id { get; set; }
        public Guid? Organization_Id { get; set; }
        public Guid? Department_Id { get; set; }
        public Guid? User_Id { get; set; }
        public string? Description { get; set; }
        public string? Document { get; set; }
        public string? Experience { get; set; }
        public int? No_Of_Question { get; set; }
        public string? Duration_Mins { get; set; }
    }
}
