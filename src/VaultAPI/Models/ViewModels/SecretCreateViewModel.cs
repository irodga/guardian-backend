// Ruta: src/VaultAPI/Models/ViewModels/SecretCreateViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VaultAPI.Models.ViewModels
{
    public class SecretCreateViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = "password"; // "password" o "fiel"

        [Required]
        public int CompanyId { get; set; }

        public DateTime? Expiration { get; set; }

        public bool RequiresApproval { get; set; }

        // Para password
        public string? SecretValue { get; set; }

        // Para fiel (archivos)
        public IFormFileCollection? Files { get; set; }
    }
}
