namespace EquipOps.Model.EquipmentCategory
{
    public class EquipmentCategoryResponse
    {
        public int TotalNumbers { get; set; } = 0;
        public List<EquipmentCategoryResponseViewModel> equipmentCategoryResponseViewModel { get; set; } = new List<EquipmentCategoryResponseViewModel>();
    }
}
