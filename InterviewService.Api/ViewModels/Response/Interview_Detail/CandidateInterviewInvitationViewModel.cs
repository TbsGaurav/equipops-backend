namespace InterviewService.Api.ViewModels.Response.Interview_Detail
{
    public class CandidateInterviewInvitationViewModel
    {
        public Guid OrganizationId { get; set; }
        public Guid CandidateId { get; set; }
        public Guid InterviewId { get; set; }
        public DateTime? InterviewDate { get; set; }
    }
}
