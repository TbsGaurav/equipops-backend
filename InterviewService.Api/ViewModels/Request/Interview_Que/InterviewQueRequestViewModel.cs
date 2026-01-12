namespace InterviewService.Api.ViewModels.Request.Interview_Que
{
    public class InterviewQueRequestViewModel
    {
        //public Guid? id { get; set; }
        //public Guid? interview_id { get; set; }
        //public string question { get; set; } = null!;
        //public int depth_level { get; set; }
        //public string description { get; set; } = null!;

        public Guid interview_id { get; set; }
        //public Guid Updated_By { get; set; }
        public List<InterviewQuestionBulkDto> questions { get; set; }
    }

    public class InterviewQueDeleteRequestViewModel
    {
        public Guid? id { get; set; }
    }

    public class InterviewQuestionBulkDto
    {
        public Guid? Id { get; set; }
        public string Question { get; set; }
        public int Depth_level { get; set; }
        //public string description { get; set; }
    }
}
