using SettingService.Api.ViewModels.Response;

using System.Text.Json;

namespace InterviewService.Api.ViewModels.Response.Interview_Form
{

    public class InterviewFormCreateUpdateResponseViewModel
    {
        public Guid? id { get; set; }
        public Guid interview_id { get; set; }
        public JsonElement form_json_data { get; set; }
    }

    public class InterviewFormListResponseViewModel : CommonParameterList
    {
        public List<InterviewFormResponseViewModel> interviewForm { get; set; } = new List<InterviewFormResponseViewModel>();
    }
    public class InterviewFormResponseViewModel : CommonParameterAllList
    {
        public Guid? id { get; set; }
        public Guid? interview_id { get; set; }
        public JsonElement form_json_data { get; set; }
    }

    public class InterviewFormDeleteResponseViewModel
    {
        public Guid? id { get; set; }
    }
}
