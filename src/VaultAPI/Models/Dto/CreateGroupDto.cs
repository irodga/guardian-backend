// Ruta: src/VaultAPI/Models/Dto/CreateGroupDto.cs
using System.ComponentModel.DataAnnotations;

namespace VaultAPI.Models.Dto
{
    public class CreateGroupDto
    {
        public string Name { get; set; } = string.Empty;  // Sin validaciones
    }
}
}
