using EquipOps.Model.ViewModelResponse;

namespace EquipOps.Model.Vendor
{
    public class VendorResponse : CommonParameterList
    {
        public List<VendorResponseViewModel> vendorResponseViewModel { get; set; }
            = new List<VendorResponseViewModel>();
    }
    public class VendorResponseViewModel : CommonParameterAllList
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
