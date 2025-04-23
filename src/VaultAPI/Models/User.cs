// src/VaultAPI/Models/User.cs
namespace VaultAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string AuthType { get; set; } = "local";
        public bool IsAdmin { get; set; }

        public ICollection<SecretAccess> SecretAccesses { get; set; } = new List<SecretAccess>();
        public ICollection<SecretAuditLog> AuditLogs { get; set; } = new List<SecretAuditLog>();
    }
}
