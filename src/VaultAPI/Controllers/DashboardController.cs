using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using VaultAPI.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace VaultAPI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly GuardianDbContext _context;

        public DashboardController(GuardianDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Forbid();  // 403 si el usuario no está autenticado
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();  // 401 si no se puede obtener el userId
            }

            var secretsCount = await _context.Secrets
                .Where(s => _context.SecretAccesses.Any(sa => sa.UserId == userId && sa.SecretId == s.Id))
                .CountAsync();

            var accessCount = await _context.SecretAccesses
                .Where(sa => sa.UserId == userId)
                .CountAsync();

            // Obtener los últimos 5 secretos creados (solo nombres)
            var recentSecrets = await _context.Secrets
                .OrderByDescending(s => s.Id)
                .Take(5)
                .Select(s => s.Name)  // Solo seleccionamos los nombres de los secretos
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
                RecentSecrets = recentSecrets,
                RecentAccesses = recentAccesses
            };

            return View(dashboardData);
        }
    }
}
