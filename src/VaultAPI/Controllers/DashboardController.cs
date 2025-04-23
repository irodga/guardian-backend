// Ruta: src/VaultAPI/Controllers/DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace VaultAPI.Controllers
{
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
            var userId = int.Parse(User.Identity!.Name!);  // O algún mecanismo para obtener el UserId

            // Obtener cantidad de secretos
            var secretsCount = await _context.Secrets
                .Where(s => _context.SecretAccesses.Any(sa => sa.UserId == userId && sa.SecretId == s.Id))
                .CountAsync();

            // Obtener cantidad de accesos
            var accessCount = await _context.SecretAccesses
                .Where(sa => sa.UserId == userId)
                .CountAsync();

            // Obtener los últimos 5 secretos creados
            var recentSecrets = await _context.Secrets
                .OrderByDescending(s => s.Id)
                .Take(5)
                .ToListAsync();

            // (Opcional) Obtener accesos recientes
            var recentAccesses = await _context.SecretAuditLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .Take(5)
                .ToListAsync();

            var dashboardData = new
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
