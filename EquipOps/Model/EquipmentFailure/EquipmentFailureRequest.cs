namespace EquipOps.Model.EquipmentFailure
{
    public class EquipmentFailureRequest
    {
        public int? failure_id { get; set; }
        public int organization_id { get; set; }
        public int equipment_id { get; set; }
        public int? subpart_id { get; set; }
        public DateTime failure_date { get; set; }
        public string? failure_type { get; set; }
        public string? description { get; set; }
        public int? downtime_minutes { get; set; }
    }
}
