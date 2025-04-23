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

    Console.WriteLine("========= DEBUG LOGIN =========");
    Console.WriteLine($"[Usuario ingresado] => '{model.Username}'");
    Console.WriteLine($"[Password ingresado] => '{model.Password}'");

    var user = _db.Users.FirstOrDefault(u =>
        u.Username == model.Username &&
        u.AuthType == "local");

    if (user == null)
    {
        Console.WriteLine("❌ Usuario no encontrado en la base");
    }
    else
    {
        Console.WriteLine($"✅ Usuario encontrado: {user.Username}");
        Console.WriteLine($"[Hash desde DB] => '{user.PasswordHash}'");

        var inputPassword = model.Password?.Trim();
        var resultado = BCrypt.Net.BCrypt.Verify(inputPassword, user.PasswordHash);
        Console.WriteLine($"[Resultado de Verify] => {resultado}");

        if (resultado)
        {
            TempData["LoginMessage"] = $"Bienvenido {user.Username}!";
            return RedirectToAction("Index", "Dashboard");
        }
    }

    Console.WriteLine("========= END DEBUG =========");
    ModelState.AddModelError("", "Usuario o contraseña incorrectos");
    return View(model);
}
    }
}