// src/VaultAPI/Models/CreateUserDto.cs
namespace VaultAPI.Models
{
    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string AuthType { get; set; } = "local";
        public bool IsAdmin { get; set; } = false;
    }
}
