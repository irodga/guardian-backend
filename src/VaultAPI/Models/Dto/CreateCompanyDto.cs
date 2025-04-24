// Ruta: src/VaultAPI/Models/Dto/CreateCompanyDto.cs
namespace VaultAPI.Models.Dto
{
    public class CreateCompanyDto
    {
        [Required(ErrorMessage = "El nombre de la empresa es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El grupo es obligatorio.")]
        public int GroupId { get; set; }  // Este será el ID del grupo al que la empresa pertenece
    }
}
