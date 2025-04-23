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
            if (ModelState.IsValid)
            {
                var user = _db.Users.FirstOrDefault(u =>
                    u.Username == model.Username &&
                    u.AuthType == "local"
                );

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    TempData["LoginMessage"] = $"Bienvenido {user.Username}!";
                    return RedirectToAction("Index", "Dashboard");
                }

                ModelState.AddModelError("", "Usuario o contraseña incorrectos");
            }

            return View(model);
        }
    }
}
