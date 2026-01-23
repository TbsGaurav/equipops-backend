namespace EquipOps.Model.DowntimeLog
{
    public class DowntimeLogRequest
    {
        public int? downtime_id { get; set; }
        public int? organization_id { get; set; }
        public int equipment_id { get; set; }
        public int? subpart_id { get; set; }
        public Guid reported_by { get; set; }
        public DateTime start_time { get; set; }
        public DateTime? end_time { get; set; }
        public string reason { get; set; }
        public string severity { get; set; }
        public decimal? cost { get; set; }
    }
}
