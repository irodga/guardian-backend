// Ruta: src/VaultAPI/Controllers/GroupsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using VaultAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace VaultAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("groups")]
    public class GroupsController : Controller  // Asegúrate de que heredes de Controller, no de ControllerBase
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

            return View("Index", groups);  // Siempre pasa los datos (o lista vacía) a la vista
        }

        // Obtener un grupo por su ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Companies)  // Incluye las empresas asociadas al grupo
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound("Grupo no encontrado.");

            return View(group);  // Pasa el grupo a la vista
        }

        // Mostrar el formulario para crear un nuevo grupo (GET)
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();  // Devuelve la vista de creación de grupo
        }

        // Crear un nuevo grupo (POST)
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateGroupDto dto)
        {
            // Verificar si la validación no pasó
            if (!ModelState.IsValid)
            {
                return View(dto);  // Retorna la vista con los errores de validación
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Unauthorized("No tienes permisos para crear un grupo.");
            }

            var group = new Group
            {
                Name = dto.Name
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            // Redirige a la acción GetAll después de crear el grupo
            return RedirectToAction(nameof(GetAll));  // Redirigir a la acción GetAll
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
