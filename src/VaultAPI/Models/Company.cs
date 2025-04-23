// src/VaultAPI/Models/Company.cs
namespace VaultAPI.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int GroupId { get; set; }

        public Group Group { get; set; } = null!;
        public ICollection<Secret> Secrets { get; set; } = new List<Secret>();
    }
}
