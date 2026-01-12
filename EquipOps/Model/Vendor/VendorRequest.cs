namespace EquipOps.Model.Vendor
{
    public class VendorRequest
    {
        public int? vendor_id { get; set; }
        public int? organization_id { get; set; }
        public string name { get; set; } = null!;
        public string? contact_name { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? service_type { get; set; }
    }
}
