// Ruta: src/VaultAPI/Controllers/CompaniesController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using VaultAPI.Models.Dto;  // Asegúrate de usar CreateCompanyDto
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VaultAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("companies")]
    public class CompaniesController : Controller
    {
        private readonly GuardianDbContext _context;

        public CompaniesController(GuardianDbContext context)
        {
            _context = context;
        }

        // Obtener todos los grupos disponibles para la selección (al crear una empresa)
        [HttpGet("create")]
        public IActionResult Create()
        {
            var groups = _context.Groups.ToList();  // Obtener todos los grupos
            ViewBag.Groups = new SelectList(groups, "Id", "Name");  // Pasar los grupos a la vista
            return View();
        }

        // Crear una nueva empresa (POST)
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateCompanyDto dto)
        {
            if (!ModelState.IsValid)
            {
                // Si hay errores en el modelo, retornar a la vista con los errores
                var groups = _context.Groups.ToList();
                ViewBag.Groups = new SelectList(groups, "Id", "Name");
                return View(dto);
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
            {
                return Unauthorized("No tienes permisos para crear una empresa.");
            }

            var company = new Company
            {
                Name = dto.Name,
                GroupId = dto.GroupId  // Asocia la empresa al grupo seleccionado
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Companies");  // Redirigir a la lista de empresas
        }

        // Obtener todas las empresas (GET)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _context.Companies
                .Include(c => c.Group)  // Incluye el grupo asociado
                .ToListAsync();

            // Convertir las empresas al DTO (si es necesario)
            var companyDtos = companies.Select(c => new CreateCompanyDto
            {
                Name = c.Name,
                GroupId = c.GroupId,
                GroupName = c.Group.Name  // Obtener el nombre del grupo asociado
            }).ToList();

            return View("Index", companyDtos);  // Pasa el DTO a la vista
        }
    }
}
