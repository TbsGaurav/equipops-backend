using System;
using System.Collections.Generic;
using System.Text;

namespace EquipOps.Model.Entities
{
	public class EquipmentEntity
	{
		public int EquipmentId { get; set; }
		public int? OrganizationId { get; set; }
		public int? CategoryId { get; set; }

		public string Name { get; set; } = string.Empty;
		public string? Type { get; set; }
		public string? QrCode { get; set; }
		public string? Location { get; set; }
		public DateOnly? PurchaseDate { get; set; }   // ✅ FIX
		public int Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
