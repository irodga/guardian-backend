// src/VaultAPI/Controllers/SecretsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace VaultAPI.Controllers
{
    [ApiController]
    [Route("secrets")]
    public class SecretsController : ControllerBase
    {
        private readonly GuardianDbContext _context;

        public SecretsController(GuardianDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var secrets = await _context.Secrets
                .Include(s => s.Company)
                .ToListAsync();

            return Ok(secrets);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var secret = await _context.Secrets
                .Include(s => s.Company)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (secret == null)
                return NotFound();

            return Ok(secret);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSecretDto dto)
        {
            var secret = new Secret
            {
                Name = dto.Name,
                Type = dto.Type,
                VaultPath = dto.VaultPath,
                Expiration = dto.Expiration,
                RequiresApproval = dto.RequiresApproval,
                CompanyId = dto.CompanyId
            };

            _context.Secrets.Add(secret);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = secret.Id }, secret);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var secret = await _context.Secrets.FindAsync(id);
            if (secret == null)
                return NotFound();

            _context.Secrets.Remove(secret);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
