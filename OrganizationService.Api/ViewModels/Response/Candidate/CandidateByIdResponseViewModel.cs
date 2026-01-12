namespace OrganizationService.Api.ViewModels.Response.Candidate
{
    public class CandidateByIdResponseViewModel
    {
        public Candidate? Candidate { get; set; }
    }

    public class Candidate
    {
        public Guid Id { get; set; }
        public Guid Interview_Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Experience { get; set; }
        public string? Skill { get; set; }
        public string? Description { get; set; }
        public string? Resume_Url { get; set; }
        public bool Is_Active { get; set; }
        public bool Is_Delete { get; set; }
        public Guid? Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public Guid? Updated_By { get; set; }
        public DateTime Updated_Date { get; set; }
    }
}
