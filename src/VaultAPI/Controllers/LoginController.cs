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
            if (User.Identity.IsAuthenticated)
                return RedirectToLocal(returnUrl);

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginModel());
        }

        // Método POST para manejar la autenticación
        [HttpPost]
        public async Task<IActionResult> Index(LoginModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model); // Recargar la vista si el modelo no es válido

            var user = _db.Users.FirstOrDefault(u => u.Username == model.Username && u.AuthType == "local");

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password?.Trim(), user.PasswordHash))
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos");
                return View(model);
            }

            // Autenticación del usuario
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Username),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            };

            var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToLocal(returnUrl); // Redirigir a la URL solicitada o al Dashboard
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Dashboard");
        }
    }
}
