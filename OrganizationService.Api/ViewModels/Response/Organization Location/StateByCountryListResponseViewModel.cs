namespace OrganizationService.Api.ViewModels.Response.OrganizationLocation
{
    public class StateByCountryListResponseViewModel
    {
        public List<States> States { get; set; } = new List<States>();

    }

    public class States
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}