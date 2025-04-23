// src/VaultAPI/Controllers/SecretAuditLogsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace VaultAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecretAuditLogsController : ControllerBase
    {
        private readonly GuardianDbContext _context;

        public SecretAuditLogsController(GuardianDbContext context)
        {
            _context = context;
        }

        // GET: api/SecretAuditLogs
        [HttpGet]
        public async Task<IActionResult> GetSecretAuditLogs()
        {
            var auditLogs = await _context.SecretAuditLogs
                .Include(log => log.User) // Asegúrate de incluir User si es necesario
                .Include(log => log.Secret) // Asegúrate de incluir Secret si es necesario
                .ToListAsync();

            return Ok(auditLogs);
        }
    }
}
