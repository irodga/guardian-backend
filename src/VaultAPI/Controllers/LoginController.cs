// Ruta: Controllers/LoginController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using System.Linq;
using BCrypt.Net;

namespace VaultAPI.Controllers
{
    public class LoginController : Controller
    {
        private readonly GuardianDbContext _db;

        public LoginController(GuardianDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index(string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;  // Mantener la URL de retorno
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginModel model, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _db.Users.FirstOrDefault(u =>
                u.Username == model.Username &&
                u.AuthType == "local");

            if (user == null)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos");
                return View(model);
            }
            
            var inputPassword = model.Password?.Trim();
            var resultado = BCrypt.Net.BCrypt.Verify(inputPassword, user.PasswordHash);

            if (resultado)
            {
                TempData["LoginMessage"] = $"Bienvenido {user.Username}!";

                // Redirigir a la URL de retorno o al dashboard si no hay una URL de retorno
                return Redirect(returnUrl ?? "/Dashboard");
            }

            ModelState.AddModelError("", "Usuario o contraseña incorrectos");
            return View(model);
        }
    }
}
