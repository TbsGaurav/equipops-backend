namespace EquipOps.Model.Organization
{
    public class Organization1ResponseViewModel
    {
        public int organization_id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string contact_email { get; set; }
        public string contact_phone { get; set; }
        public DateTime? created_date { get; set; }
        public DateTime? updated_date { get; set; }
    }
}
