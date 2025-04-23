// src/VaultAPI/Controllers/DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using VaultAPI.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace VaultAPI.Controllers
{
    [Authorize]  // Asegura que solo los usuarios autenticados accedan a este controlador
    public class DashboardController : Controller
    {
        private readonly GuardianDbContext _context;

        public DashboardController(GuardianDbContext context)
        {
            _context = context;
        }

        // GET: /Dashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Asegúrate de que el usuario esté autenticado y que el User.Identity.Name no sea nulo
            if (!User.Identity.IsAuthenticated)
            {
                // Si no está autenticado, redirige al login o responde con 403
                return Forbid();  // Esto debería devolver un código 403
            }

            var userId = User.Identity.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();  // Si el userId es nulo o vacío, devuelve 401
            }

            // Convertir userId de string a int de forma segura
            if (!int.TryParse(userId, out int parsedUserId))
            {
                return Unauthorized();  // Si no se puede convertir, redirigir a 401
            }

            // Obtener cantidad de secretos accesibles
            var secretsCount = await _context.Secrets
                .Where(s => _context.SecretAccesses.Any(sa => sa.UserId == parsedUserId && sa.SecretId == s.Id))
                .CountAsync();

            // Obtener cantidad de accesos de secretos
            var accessCount = await _context.SecretAccesses
                .Where(sa => sa.UserId == parsedUserId)
                .CountAsync();

            // Obtener los últimos 5 secretos creados
            var recentSecrets = await _context.Secrets
                .OrderByDescending(s => s.Id)
                .Take(5)
                .ToListAsync();

            // Obtener accesos recientes de auditoría (SecretAuditLogs)
            var recentAccesses = await _context.SecretAuditLogs
                .Where(log => log.UserId == parsedUserId)
                .OrderByDescending(log => log.Timestamp)
                .Take(5)
                .ToListAsync();

            // Preparar el modelo de datos para la vista
            var dashboardData = new DashboardViewModel
            {
                SecretsCount = secretsCount,
                AccessCount = accessCount,
                RecentSecrets = recentSecrets,
                RecentAccesses = recentAccesses
            };

            return View(dashboardData);  // Enviar el ViewModel a la vista
        }
    }
}
