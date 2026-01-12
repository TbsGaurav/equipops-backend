using Newtonsoft.Json;

namespace OrganizationService.Api.ViewModels.Request.Candidate
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
