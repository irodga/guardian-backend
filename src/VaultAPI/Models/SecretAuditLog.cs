// src/VaultAPI/Models/SecretAuditLog.cs
namespace VaultAPI.Models
{
    public class SecretAuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SecretId { get; set; }
        public string Action { get; set; } = string.Empty; // accessed, requested, approved, rejected
        public bool Success { get; set; } = true;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Details { get; set; } = string.Empty;

        public User User { get; set; } = null!;
        public Secret Secret { get; set; } = null!;
    }
}
