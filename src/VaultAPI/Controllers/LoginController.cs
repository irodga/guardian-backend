using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using System.Linq;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;  // Importar para ClaimsIdentity

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

            // Verificar si el modelo es válido
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

            // Buscar el usuario en la base de datos
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

                // Verificar la contraseña usando BCrypt
                var inputPassword = model.Password?.Trim();
                var resultado = BCrypt.Net.BCrypt.Verify(inputPassword, user.PasswordHash);
                Console.WriteLine($"[Resultado de Verify] => {resultado}");

                if (resultado)
                {
                    // Crear los claims para la sesión
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  // Asegúrate de que el user.Id esté aquí
                        new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Iniciar la sesión del usuario
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    // Redirigir al Dashboard
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
