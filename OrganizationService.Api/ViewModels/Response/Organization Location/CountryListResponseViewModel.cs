namespace OrganizationService.Api.ViewModels.Response.OrganizationLocation
{
    public class CountryListResponseViewModel
    {
        public List<Countries> Countries { get; set; } = new List<Countries>();
    }

    public class Countries
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}