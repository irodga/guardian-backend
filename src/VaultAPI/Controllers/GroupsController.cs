// Ruta: src/VaultAPI/Controllers/GroupsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using VaultAPI.Models.Dto;  // Asegúrate de que esta importación esté presente
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        // Obtener todos los grupos con sus empresas asociadas
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var groups = await _context.Groups
                .Include(g => g.Companies)  // Incluye las empresas asociadas al grupo
                .ToListAsync();

            // Si no hay grupos, pasa una lista vacía a la vista
            return View(groups);  // Siempre pasa los datos (o lista vacía) a la vista
        }

        // Crear un nuevo grupo (solo accesible para administradores)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGroupDto dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Unauthorized("No tienes permisos para crear un grupo.");
            }

            if (dto == null || string.IsNullOrEmpty(dto.Name))
            {
                return BadRequest("Nombre del grupo no válido.");
            }

            var group = new Group
            {
                Name = dto.Name
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
        }

        // Eliminar un grupo (solo accesible para administradores)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Unauthorized("No tienes permisos para eliminar un grupo.");
            }

            var group = await _context.Groups.FindAsync(id);
            if (group == null)
                return NotFound("Grupo no encontrado.");

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
