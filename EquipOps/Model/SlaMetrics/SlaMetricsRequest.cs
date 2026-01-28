namespace EquipOps.Model.SlaMetrics
{
	public sealed class SlaMetricsRequest
	{
		public int? SlaId { get; set; }
		public int OrganizationId { get; set; }
		public int EquipmentId { get; set; }
		public int SubpartId { get; set; }
		public DateTime PeriodStart { get; set; }
		public DateTime PeriodEnd { get; set; }
		public int DowntimeMinutes { get; set; }
		public bool SlaBreached { get; set; }
	}
}
