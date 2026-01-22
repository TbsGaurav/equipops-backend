namespace EquipOps.Model.Role
{
    public class RoleRequest
    {
        public int? role_id { get; set; }
        public string role_name { get; set; }
        public string description { get; set; }
        public bool is_active { get; set; } = true;
        public Guid? created_by { get; set; }
    }
}
