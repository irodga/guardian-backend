// Ruta: Controllers/LoginController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;

namespace VaultAPI.Controllers
{
    public class LoginController : Controller
    {
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
                // Simulación de login
                if (model.Username == "admin" && model.Password == "123")
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                ModelState.AddModelError("", "Usuario o contraseña incorrectos");
            }
            return View(model);
        }
    }
}
