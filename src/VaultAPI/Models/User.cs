namespace VaultAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string AuthType { get; set; } // "local" o "saml"
        public bool IsAdmin { get; set; }

        public ICollection<SecretAccess> SecretAccesses { get; set; }
        public ICollection<SecretAuditLog> AuditLogs { get; set; }
    }
}
