// Ruta: src/VaultAPI/Models/Dto/CreateSecretDto.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace VaultAPI.Models.Dto
{
    public class CreateSecretDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;  // Tipo: "password" o "file"

        public string? Value { get; set; }  // Para los secretos de tipo "password"

        public List<IFormFile>? Files { get; set; }  // Para los secretos de tipo "file" (fiel)

        public DateTime? Expiration { get; set; }  // Fecha de expiración del secreto

        public bool RequiresApproval { get; set; }  // Si requiere aprobación para acceder

        [Required]
        public int CompanyId { get; set; }  // ID de la empresa asociada
    }
}
