// src/VaultAPI/Controllers/SecretAuditLogsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace VaultAPI.Controllers
{
    [ApiController]
    [Route("audit-logs")]
    public class SecretAuditLogsController : ControllerBase
    {
        private readonly GuardianDbContext _context;

        public SecretAuditLogsController(GuardianDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _context.SecretAuditLogs
                .Include(l => l.User)
                .Include(l => l.Secret)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            return Ok(logs);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAuditLogDto dto)
        {
            var log = new SecretAuditLog
            {
                UserId = dto.UserId,
                SecretId = dto.SecretId,
                Action = dto.Action,
                Success = dto.Success,
                Details = dto.Details,
                Timestamp = DateTime.UtcNow
            };

            _context.SecretAuditLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(log);
        }
    }
}
