// src/VaultAPI/Models/CreateSecretAccessDto.cs
namespace VaultAPI.Models
{
    public class CreateSecretAccessDto
    {
        public int UserId { get; set; }
        public int SecretId { get; set; }
        public string Permission { get; set; } = "read";
    }
}
