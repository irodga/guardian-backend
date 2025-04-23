using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using VaultAPI.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
            // Verificar que el usuario esté autenticado y obtener el userId de los claims
            if (!User.Identity.IsAuthenticated)
            {
                return Forbid();  // Esto devolverá 403 si el usuario no está autenticado
            }

            // Obtener el userId de los claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);  // Usar NameIdentifier si está disponible
            if (userIdClaim == null || !int.TryParse(userIdClaim?.Value, out int userId))  // Usando operador de acceso seguro
            {
                return Unauthorized();  // 401 si no se puede obtener el userId
            }

            // Obtener cantidad de secretos accesibles
            var secretsCount = await _context.Secrets
                .Where(s => _context.SecretAccesses.Any(sa => sa.UserId == userId && sa.SecretId == s.Id))
                .CountAsync();

            // Obtener cantidad de accesos de secretos
            var accessCount = await _context.SecretAccesses
                .Where(sa => sa.UserId == userId)
                .CountAsync();

            // Obtener los últimos 5 secretos creados (solo nombres)
            var recentSecrets = await _context.Secrets
                .OrderByDescending(s => s.Id)
                .Take(5)
                .Select(s => s.Name)  // Solo los nombres de los secretos
                .ToListAsync();

            // Obtener accesos recientes con relación a Secret
            var recentAccesses = await _context.SecretAuditLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .Take(5)
                .Include(log => log.Secret)  // Incluimos la relación con Secret
                .ToListAsync();

            var dashboardData = new DashboardViewModel
            {
                SecretsCount = secretsCount,
                AccessCount = accessCount,
                RecentSecrets = recentSecrets,  // Lista de nombres de secretos
                RecentAccesses = recentAccesses  // Lista de accesos recientes con los secretos relacionados
            };

            return View(dashboardData);
        }
    }
}
