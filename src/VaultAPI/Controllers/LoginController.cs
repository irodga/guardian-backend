// src/VaultAPI/Controllers/LoginController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using System.Linq;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace VaultAPI.Controllers
{
    public class LoginController : Controller
    {
        private readonly GuardianDbContext _db;

        public LoginController(GuardianDbContext db)
        {
            _db = db;
        }

        // Este método maneja el acceso al login y redirige si el usuario ya está autenticado
        [HttpGet]
        public IActionResult Index(string returnUrl)
        {
            // Si el usuario ya está autenticado, redirigir directamente al Dashboard o al returnUrl
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl); // Redirige a la URL solicitada o al Dashboard
            }

            // Pasamos el returnUrl a la vista
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Método POST para manejar la autenticación
        [HttpPost]
        public async Task<IActionResult> Index(LoginModel model, string returnUrl)
        {
            // Validamos que las credenciales sean correctas solo si el modelo es válido
            if (ModelState.IsValid)
            {
                var user = _db.Users.FirstOrDefault(u =>
                    u.Username == model.Username &&
                    u.AuthType == "local");

                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password?.Trim(), user.PasswordHash))
                {
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos");
                    return View(model); // Si las credenciales son incorrectas, recargar la vista con el mensaje de error
                }

                // Si las credenciales son correctas, autenticar al usuario
                var claims = new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
                };

                var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new System.Security.Claims.ClaimsPrincipal(identity);

                // Iniciar sesión
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Redirigir a la página solicitada o al Dashboard si no hay ReturnUrl
                return RedirectToLocal(returnUrl);
            }

            return View(model); // Si el ModelState no es válido, recargar la vista con los errores
        }

        // Función para redirigir a la URL solicitada si es válida o a la raíz
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) // Si es una URL local, redirige
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard"); // Si no, redirige a Dashboard
            }
        }
    }
}
