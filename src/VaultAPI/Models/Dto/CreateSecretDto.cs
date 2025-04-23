// Ruta: src/VaultAPI/Models/Dto/CreateSecretDto.cs
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace VaultAPI.Models.Dto
{
    public class CreateSecretDto
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
        public string? Value { get; set; }

        // Para fiel (archivos)
        public IFormFileCollection? Files { get; set; }

        // VaultPath (lo que usaremos para la ruta de almacenamiento)
        public string VaultPath { get; set; } = string.Empty;
    }
}
