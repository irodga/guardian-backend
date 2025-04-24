// Ruta: src/VaultAPI/Models/CreateGroupDto.cs
using System.ComponentModel.DataAnnotations;

namespace VaultAPI.Models
{
    public class CreateGroupDto
    {
        [Required(ErrorMessage = "El nombre del grupo es obligatorio.")]
        [StringLength(120, ErrorMessage = "El nombre del grupo no puede exceder los 120 caracteres.")]
        public string Name { get; set; } = string.Empty;
    }
}