using Newtonsoft.Json;

namespace InterviewService.Api.ViewModels.Response.Interview
{
    public class InterviewTokenResponseViewModel
    {
        [JsonProperty("CandidateInterviewInvitationId")]
        public Guid Candidate_interview_invitation_id { get; set; }

        [JsonProperty("InterviewDate")]
        public DateTime Interview_date { get; set; }      
    }
}
