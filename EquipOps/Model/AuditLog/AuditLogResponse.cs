namespace EquipOps.Model.AuditLog
{
    public class AuditLogResponse
    {
        public long AuditId { get; set; }
        public int OrganizationId { get; set; }
        public int UserId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public object? OldData { get; set; }
        public object? NewData { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
