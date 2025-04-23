// src/VaultAPI/Controllers/GroupsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace VaultAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("groups")]
    public class GroupsController : ControllerBase
    {
        private readonly GuardianDbContext _context;

        public GroupsController(GuardianDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var groups = await _context.Groups
                .Include(g => g.Companies)
                .ToListAsync();

            return Ok(groups);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Companies)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound();

            return Ok(group);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateGroupDto dto)
        {
            var group = new Group
            {
                Name = dto.Name
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
                return NotFound();

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
