namespace EquipOps.Model.Equipment
{
	public class EquipmentRequest
	{
		public int EquipmentId { get; set; }
		public int? OrganizationId { get; set; }
		public int? CategoryId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Type { get; set; }
		public string? QrCode { get; set; }
		public string? Location { get; set; }
		public DateTime? PurchaseDate { get; set; }
		public int Status { get; set; }
	}
}
