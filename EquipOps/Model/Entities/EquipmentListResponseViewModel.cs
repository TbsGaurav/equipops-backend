using EquipOps.Model.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipOps.Model.Entities
{
	public class EquipmentListResponseViewModel
	{
		public int TotalNumbers { get; set; }
		public IReadOnlyList<EquipmentDisplayDto> EquipmentData { get; set; }
	}
}
