namespace EquipOps.Model.DashboardData
{
    public class DashboardRebuildRequest
    {
        public int organization_id { get; set; }
        public int downtime_category_id { get; set; }
        public int workorder_category_id { get; set; }
    }
}
