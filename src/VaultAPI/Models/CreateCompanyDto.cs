// src/VaultAPI/Models/CreateCompanyDto.cs
namespace VaultAPI.Models
{
    public class CreateCompanyDto
    {
        public string Name { get; set; } = string.Empty;
        public int GroupId { get; set; }
    }
}
