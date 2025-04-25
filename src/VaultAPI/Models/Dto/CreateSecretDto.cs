// Ruta: src/VaultAPI/Models/Dto/CreateSecretDto.cs
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace VaultAPI.Models.Dto
{
    public class CreateSecretDto  
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;  // Tipo: "password" o "file"

        public string? Value { get; set; }  // Para los secretos de tipo "password"

        public List<IFormFile>? Files { get; set; }  // Para los secretos de tipo "file" (fiel)

        public DateTime? Expiration { get; set; }  // Fecha de expiración del secreto

        public bool RequiresApproval { get; set; }  // Si requiere aprobación para acceder

        public int CompanyId { get; set; }  // ID de la empresa asociada

        public List<Company>? Companies { get; set; }  // Lista de empresas asociadas
    }
}