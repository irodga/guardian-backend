// src/VaultAPI/Controllers/LoginController.cs
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
        public IActionResult Index(string returnUrl)
        {
            // Pasamos el returnUrl a la vista a través de ViewData
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginModel model)
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
            var result = BCrypt.Net.BCrypt.Verify(inputPassword, user.PasswordHash);

            if (result)
            {
                TempData["LoginMessage"] = $"Bienvenido {user.Username}!";

                // Redirigir a ReturnUrl o a Dashboard por defecto
                var returnUrl = Request.Query["ReturnUrl"].ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    returnUrl = "/Dashboard";  // Redirigir a Dashboard por defecto si no hay ReturnUrl
                }

                return Redirect(returnUrl);  // Redirigir al ReturnUrl
            }

            ModelState.AddModelError("", "Usuario o contraseña incorrectos");
            return View(model);
        }
    }
}
