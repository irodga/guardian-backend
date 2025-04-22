namespace VaultAPI.Models
{
    public class SecretAccess
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SecretId { get; set; }
        public string Permission { get; set; } // read, write, admin
        public DateTime GrantedAt { get; set; }

        public User User { get; set; }
        public Secret Secret { get; set; }
    }
}
