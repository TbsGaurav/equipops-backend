using SettingService.Api.ViewModels.Response;

namespace InterviewService.Api.ViewModels.Response.Interview_Que
{

    public class InterviewQueCreateUpdateResponseViewModel
    {
        //public Guid interview_id { get; set; }
        //public List<InterviewQuestionBulkDto> questions { get; set; }
    }
    //public class InterviewQuestionResponseBulkDto
    //{
    //    public Guid? id { get; set; }
    //    public string question { get; set; }
    //    public int depth_level { get; set; }
    //    public string description { get; set; }
    //}

    public class InterviewQueListResponseViewModel : CommonParameterList
    {
        public List<InterviewQueResponseViewModel> interviewQue { get; set; } = new List<InterviewQueResponseViewModel>();
    }
    public class InterviewQueResponseViewModel : CommonParameterAllList
    {
        public Guid? id { get; set; }
        public Guid? interview_id { get; set; }
        public string question { get; set; } = null!;
        public int depth_level { get; set; }
        //public string description { get; set; } = null!;   
    }

    public class InterviewQueDeleteResponseViewModel
    {
        public Guid? id { get; set; }
    }
}
