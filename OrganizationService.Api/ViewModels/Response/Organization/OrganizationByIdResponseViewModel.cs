namespace OrganizationService.Api.ViewModels.Response.Organization
{
    public class OrganizationByIdResponseViewModel
    {
        public Organization? Organization { get; set; }

    }

    public class Organization
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Website_Url { get; set; }
        public string? Email { get; set; }
        public string? Phone_No { get; set; }
        public string? First_Name { get; set; }
        public string? Last_Name { get; set; }
        public Guid? Industry_Type_Id { get; set; }
        public string? Number_Of_Employees { get; set; }
        public List<OrganizationLocationResponse> Locations { get; set; } = new List<OrganizationLocationResponse>();
        public List<OrganizationDepartmentResponse> Departments { get; set; } = new List<OrganizationDepartmentResponse>();
        public bool Is_Active { get; set; }
        public bool Is_Delete { get; set; }
        public Guid? Created_By { get; set; }
        public DateTime Created_Date { get; set; }
        public Guid? Updated_By { get; set; }
        public DateTime Updated_Date { get; set; }
    }

    public class OrganizationLocationResponse
    {
        public Guid Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Zip_Code { get; set; } = string.Empty;
        public Guid Country_Id { get; set; }
        public Guid State_Id { get; set; }
        public Guid City_Id { get; set; }
        public bool Is_Primary { get; set; }
        public bool Is_Active { get; set; }
    }
    public class OrganizationDepartmentResponse
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}