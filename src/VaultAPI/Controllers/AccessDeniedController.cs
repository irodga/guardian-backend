// Ruta: src/VaultAPI/Controllers/AccessDeniedController.cs
using Microsoft.AspNetCore.Mvc;

namespace VaultAPI.Controllers
{
    public class AccessDeniedController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();  // Muestra una vista de acceso denegado
        }
    }
}
