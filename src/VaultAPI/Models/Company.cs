namespace VaultAPI.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GroupId { get; set; }

        public Group Group { get; set; }
        public ICollection<Secret> Secrets { get; set; }
    }
}
