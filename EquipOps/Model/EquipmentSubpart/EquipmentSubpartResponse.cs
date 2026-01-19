namespace EquipOps.Model.EquipmentSubpart
{
    public class EquipmentSubpartResponse
    {
        public int subpart_id { get; set; }
        public int equipment_id { get; set; }
        public string? equipment_name { get; set; }
        public string name { get; set; } = string.Empty;
        public string? description { get; set; }
        public bool? status { get; set; }
        public string? qr_code { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}
