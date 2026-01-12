using InterviewService.Api.ViewModels.Request.Interview_Que;

namespace InterviewService.Api.ViewModels.Response.Interview
{
    public class InterviewByIdResponseViewModel
    {
        public Interview? Interview { get; set; }
        public List<InterviewQuestionBulkDto> questions { get; set; }
    }

    public class Interview
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid Interviewer_Id { get; set; }
        public Guid Interview_Type_Id { get; set; }
        public Guid Work_Mode_Id { get; set; }
        public Guid Department_Id { get; set; }
        public Guid Organization_Id { get; set; }
        public Guid User_Id { get; set; }
        public Guid Interview_Form_Id { get; set; }
        public string? Description { get; set; }
        public string? Document { get; set; }
        public bool? Job_status { get; set; }
        public string? Experience { get; set; }
        public int? No_Of_Question { get; set; }
        public string? Duration_Mins { get; set; }
        public bool Is_Active { get; set; }
        public bool Is_Delete { get; set; }
        public Guid? Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public Guid? Updated_By { get; set; }
        public DateTime Updated_Date { get; set; }
    }
}
