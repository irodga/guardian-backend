// src/VaultAPI/Models/SecretAccess.cs
namespace VaultAPI.Models
{
    public class SecretAccess
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SecretId { get; set; }
        public string Permission { get; set; } = "read"; // valores posibles: read, write, admin
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public Secret Secret { get; set; } = null!;
    }
}
