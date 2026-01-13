namespace EquipOps.Model.EquipmentCategory
{
    public class EquipmentCategoryResponseViewModel
    {
        public int category_id { get; set; }
        public int organization_id { get; set; }
        public string category_name { get; set; }
        public string organization_name { get; set; }
        public string? description { get; set; }
        public DateTime? created_date { get; set; }
        public DateTime? updated_date { get; set; }
    }
}
