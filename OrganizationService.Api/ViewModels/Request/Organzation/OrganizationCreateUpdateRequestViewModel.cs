namespace OrganizationService.Api.ViewModels.Request.Organzation
{
    public class OrganizationCreateUpdateRequestViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Website_url { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone_no { get; set; } = string.Empty;
        public string First_name { get; set; } = string.Empty;
        public string Last_name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Guid? Industry_type_id { get; set; }
        public string? Number_of_employees { get; set; }
        public List<LocationCreateUpdateItemViewModel> Locations { get; set; } = new();
        public List<DepartmentsCreateUpdateItemViewModel> Departments { get; set; } = new();

        public class LocationCreateUpdateItemViewModel
        {
            public Guid? Id { get; set; }
            public string Address { get; set; } = string.Empty;
            public Guid? City_id { get; set; }
            public Guid? Country_id { get; set; }
            public Guid? State_id { get; set; }
            public bool Is_Primary { get; set; }
            public string Zip_code { get; set; } = string.Empty;
        }
        public class DepartmentsCreateUpdateItemViewModel
        {
            public Guid? Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public Guid? Industry_type_id { get; set; }
        }
    }
}
