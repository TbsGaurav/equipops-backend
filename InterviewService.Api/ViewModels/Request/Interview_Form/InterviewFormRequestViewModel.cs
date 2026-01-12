using System.Text.Json;

namespace InterviewService.Api.ViewModels.Request.Interview_Form
{
    public class InterviewFormRequestViewModel
    {
        public Guid? id { get; set; }
        public Guid interview_id { get; set; }
        public JsonElement form_json_data { get; set; }
    }

    public class InterviewFormDeleteRequestViewModel
    {
        public Guid? id { get; set; }
    }
}
