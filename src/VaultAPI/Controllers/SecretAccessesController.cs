// src/VaultAPI/Controllers/SecretAccessesController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace VaultAPI.Controllers
{
    [ApiController]
    [Route("accesses")]
    public class SecretAccessesController : ControllerBase
    {
        private readonly GuardianDbContext _context;

        public SecretAccessesController(GuardianDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var accesses = await _context.SecretAccesses
                .Include(a => a.User)
                .Include(a => a.Secret)
                .ToListAsync();

            return Ok(accesses);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSecretAccessDto dto)
        {
            var access = new SecretAccess
            {
                UserId = dto.UserId,
                SecretId = dto.SecretId,
                Permission = dto.Permission
            };

            _context.SecretAccesses.Add(access);
            await _context.SaveChangesAsync();

            return Ok(access);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var access = await _context.SecretAccesses.FindAsync(id);
            if (access == null)
                return NotFound();

            _context.SecretAccesses.Remove(access);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
