// src/VaultAPI/Models/CreateSecretDto.cs
namespace VaultAPI.Models
{
    public class CreateSecretDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string VaultPath { get; set; } = string.Empty;
        public DateTime? Expiration { get; set; }
        public bool RequiresApproval { get; set; }
        public int CompanyId { get; set; }
    }
}
