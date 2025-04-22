namespace VaultAPI.Models
{
    public class Secret
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // password o fiel
        public string VaultPath { get; set; }
        public DateTime? Expiration { get; set; }
        public bool RequiresApproval { get; set; }
        public int CompanyId { get; set; }

        public Company Company { get; set; }
        public ICollection<SecretAccess> SecretAccesses { get; set; }
        public ICollection<SecretAuditLog> AuditLogs { get; set; }
    }
}