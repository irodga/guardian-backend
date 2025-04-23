// src/VaultAPI/Models/CreateAuditLogDto.cs
namespace VaultAPI.Models
{
    public class CreateAuditLogDto
    {
        public int UserId { get; set; }
        public int SecretId { get; set; }
        public string Action { get; set; } = "accessed";
        public bool Success { get; set; } = true;
        public string Details { get; set; } = string.Empty;
    }
}
