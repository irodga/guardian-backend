// src/VaultAPI/Models/Secret.cs
namespace VaultAPI.Models
{
    public class Secret
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "password", "fiel", etc.
        public string VaultPath { get; set; } = string.Empty;
        public DateTime? Expiration { get; set; }
        public bool RequiresApproval { get; set; }
        public int CompanyId { get; set; }

        public Company Company { get; set; } = null!;
        public ICollection<SecretAccess> SecretAccesses { get; set; } = new List<SecretAccess>();
        public ICollection<SecretAuditLog> AuditLogs { get; set; } = new List<SecretAuditLog>();
    }
}
