using Newtonsoft.Json;

namespace InterviewService.Api.ViewModels.Request.Interview
{
    public class InterviewTokenRequestViewModel
    {
        [JsonProperty("InterviewId")]
        public Guid Interview_id { get; set; }

        [JsonProperty("CandidateId")]
        public Guid Candidate_id { get; set; }

        [JsonProperty("InterviewDate")]
        public DateTime Interview_date { get; set; }
    }
}
