namespace EquipOps.Model.EquipmentCategory
{
    public class EquipmentCategoryRequest
    {
        public int? category_id { get; set; }
        public int organization_id { get; set; }
        public string category_name { get; set; }
        public string? description { get; set; }
    }
}
