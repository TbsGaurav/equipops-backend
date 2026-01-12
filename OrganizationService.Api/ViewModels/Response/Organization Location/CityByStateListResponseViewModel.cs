namespace OrganizationService.Api.ViewModels.Response.OrganizationLocation
{
    public class CityByStateListResponseViewModel
    {
        public List<Cities> Cities { get; set; } = new List<Cities>();
    }

    public class Cities
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}