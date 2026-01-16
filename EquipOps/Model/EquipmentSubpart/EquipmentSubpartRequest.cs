namespace EquipOps.Model.EquipmentSubpart
{
    public class EquipmentSubpartRequest
    {
        public int? subpart_id { get; set; }
        public int equipment_id { get; set; }
        public string subpart_name { get; set; } = string.Empty;
        public string? description { get; set; }
        public string? status { get; set; }
        public string? qr_code { get; set; }
    }
}
