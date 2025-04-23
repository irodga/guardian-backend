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
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginModel model)
        {
            Console.WriteLine("🔁 POST /Login/Index recibido");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState inválido");
                foreach (var error in ModelState)
                {
                    foreach (var sub in error.Value.Errors)
                    {
                        Console.WriteLine($"➡️ Campo: {error.Key} - Error: {sub.ErrorMessage}");
                    }
                }

                return View(model);
            }

            Console.WriteLine($"➡️ Username recibido: {model.Username}");

            var user = _db.Users.FirstOrDefault(u =>
                u.Username == model.Username &&
                u.AuthType == "local");

            if (user == null)
            {
                Console.WriteLine("❌ Usuario no encontrado en la base");
            }
            else
            {
                Console.WriteLine("✅ Usuario encontrado");
                bool passwordCorrecto = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

                Console.WriteLine(passwordCorrecto
                    ? "🔓 Contraseña correcta"
                    : "🔐 Contraseña incorrecta");

                if (passwordCorrecto)
                {
                    TempData["LoginMessage"] = $"Bienvenido {user.Username}!";
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            ModelState.AddModelError("", "Usuario o contraseña incorrectos");
            return View(model);
        }
    }
}
