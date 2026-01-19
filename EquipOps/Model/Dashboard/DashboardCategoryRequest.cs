namespace EquipOps.Model.Dashboard
{
	public class DashboardCategoryRequest
	{
		public int? DashboardCategoryId { get; set; }
		public int OrganizationId { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }
	}
}
