// src/VaultAPI/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace VaultAPI.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly GuardianDbContext _context;

        public AuthController(GuardianDbContext context)
        {
            _context = context;
        }

        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.AuthType == "local");

            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
                return Unauthorized("Usuario o contraseña inválidos.");

            var incomingHash = HashPassword(request.Password);
            if (user.PasswordHash != incomingHash)
                return Unauthorized("Usuario o contraseña inválidos.");

            return Ok(new { auth = "ok", username = user.Username, isAdmin = user.IsAdmin });
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }
    }
}
