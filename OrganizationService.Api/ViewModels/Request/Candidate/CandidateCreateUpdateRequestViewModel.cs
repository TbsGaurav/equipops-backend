namespace OrganizationService.Api.ViewModels.Request.Candidate
{
    public class CandidateCreateUpdateRequestViewModel
    {
        public CanidateDetail json_form_data { get; set; }
    }

    public class CanidateDetail
    {
        public Guid? Id { get; set; }
        public Guid InterviewId { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone_Number { get; set; }
        public List<CandidateEducationDetail> Education { get; set; }
        public List<CandidateExperienceDetail> Experience { get; set; }
        public string[] Skills { get; set; }
        public IFormFile? Resume { get; set; }
        public int CandidateUploadType { get; set; } = 0;
    }
    public class CandidateEducationDetail
    {
        public string Passing_Year { get; set; }
        public string university_name { get; set; }
    }
    public class CandidateExperienceDetail
    {
        public string Duration { get; set; }
        public string Company_Name { get; set; }
        public bool Is_Current_Company { get; set; }
    }

    public class CanidateDirectInvitationDetail
    {
        public Guid? Id { get; set; }
        public Guid InterviewId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public IFormFile? Resume { get; set; }
    }
    public class CanidateResumeList
    {
        public List<IFormFile> ResumeDetail { get; set; }
        public int CandidateUploadType { get; set; } = 0;
    }

}