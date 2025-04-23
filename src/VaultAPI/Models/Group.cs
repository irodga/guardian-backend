// src/VaultAPI/Models/Group.cs
namespace VaultAPI.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Company> Companies { get; set; } = new List<Company>();
    }
}
