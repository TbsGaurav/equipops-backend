namespace OrganizationService.Api.ViewModels.Response.Organization
{
    public class OrganizationListResponseViewModel
    {
        public int TotalNumbers { get; set; }
        public List<OrganizationData> OrganizationData { get; set; } = new List<OrganizationData>();
    }

    public class OrganizationData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Website_Url { get; set; }
        public string? Email { get; set; }
        public string? Phone_No { get; set; }
        public Guid? Industry_Type_Id { get; set; }
        public string? Number_Of_Employees { get; set; }
        public string? Location { get; set; }
        public string? Department { get; set; }
        public bool Is_Active { get; set; }
        public bool Is_Delete { get; set; }
        public Guid? Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public Guid? Updated_By { get; set; }
        public DateTime Updated_Date { get; set; }
        public string? Industry_Type { get; set; }
    }
}
