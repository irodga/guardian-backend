namespace VaultAPI.Models
{
    public class SecretAuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SecretId { get; set; }
        public string Action { get; set; } // accessed, requested, approved, rejected
        public bool Success { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }

        public User User { get; set; }
        public Secret Secret { get; set; }
    }
}
