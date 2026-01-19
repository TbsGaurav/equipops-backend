namespace EquipOps.Model.Permission
{
    public class PermissionRequest
    {
        public int? permission_id { get; set; }
        public string permission_code { get; set; } = string.Empty;
        public string? description { get; set; }
        public bool? is_active { get; set; }
    }
}
