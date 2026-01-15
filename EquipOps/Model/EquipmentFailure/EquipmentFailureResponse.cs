namespace EquipOps.Model.EquipmentFailure
{
    public class EquipmentFailureResponse
    {
        public int TotalNumbers { get; set; }
        public List<EquipmentFailureResponseViewModel> equipmentFailureResponseViewModel { get; set; } = new();
    }
    public class EquipmentFailureResponseViewModel
    {
        public int failure_id { get; set; }
        public int organization_id { get; set; }
        public string? organization_name { get; set; }
        public int equipment_id { get; set; }
        public string? equipment_name { get; set; }
        public int? subpart_id { get; set; }
        public string? subpart_name { get; set; }
        public DateTime failure_date { get; set; }
        public string? failure_type { get; set; }
        public string? description { get; set; }
        public int? downtime_minutes { get; set; }
        public DateTime created_date { get; set; }
        public DateTime? updated_date { get; set; }
    }
}
