// Ruta: src/VaultAPI/Models/Dto/CompanyDto.cs
namespace VaultAPI.Models.Dto
{
    public class CreateCompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;  // Nombre del grupo asociado
    }
}
