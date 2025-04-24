// Ruta: src/VaultAPI/Controllers/CompaniesController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using VaultAPI.Models.Dto;  
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace VaultAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("companies")]
    public class CompaniesController : Controller
    {
        private readonly GuardianDbContext _context;
        private readonly ILogger<CompaniesController> _logger;  // Para agregar los logs

        public CompaniesController(GuardianDbContext context, ILogger<CompaniesController> logger)
        {
            _context = context;
            _logger = logger;  // Inyectamos el logger
        }

        // Obtener todos los grupos disponibles para la selección (al crear una empresa)
        [HttpGet("create")]
        public IActionResult Create()
        {
            var groups = _context.Groups.ToList();  // Obtener todos los grupos
            _logger.LogInformation("Obteniendo grupos de la base de datos. Total de grupos encontrados: {GroupCount}", groups.Count);

            if (groups == null || !groups.Any())
            {
                _logger.LogWarning("No se encontraron grupos en la base de datos.");
                return View("Error");  // Redirigir a una vista de error si no hay grupos disponibles
            }

            // Creamos la lista de SelectListItem en lugar de SelectList
            var groupItems = groups.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Name
            }).ToList();

            ViewBag.Groups = groupItems;  // Pasar la lista de SelectListItem a la vista
            return View();
        }

        // Crear una nueva empresa (POST)
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateCompanyDto dto)
        {
            // Log para verificar los datos recibidos en el formulario
            _logger.LogInformation("Recibiendo datos de la nueva empresa: Nombre={CompanyName}, GrupoId={GroupId}", dto.Name, dto.GroupId);

            if (!ModelState.IsValid)
            {
                // Log si el modelo no es válido
                _logger.LogWarning("El modelo recibido no es válido. Errores: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                // Si hay errores en el modelo, retornar a la vista con los errores
                var groups = _context.Groups.ToList();
                var groupItems = groups.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                }).ToList();

                ViewBag.Groups = groupItems;
                return View(dto);
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
            {
                _logger.LogWarning("El usuario no tiene permisos de administrador para crear la empresa.");
                return Unauthorized("No tienes permisos para crear una empresa.");
            }

            var company = new Company
            {
                Name = dto.Name,
                GroupId = dto.GroupId  // Asocia la empresa al grupo seleccionado
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // Log cuando se crea la empresa
            _logger.LogInformation("Empresa creada exitosamente: {CompanyName}, GrupoId={GroupId}", company.Name, company.GroupId);

            return RedirectToAction("Index", "Companies");  // Redirigir a la lista de empresas
        }

        // Obtener todas las empresas (GET)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _context.Companies
                .Include(c => c.Group)  // Incluye el grupo asociado
                .ToListAsync();

            // Log para verificar la cantidad de empresas obtenidas
            _logger.LogInformation("Obteniendo empresas. Total de empresas encontradas: {CompanyCount}", companies.Count);

            // Convertir las empresas a CreateCompanyDto antes de pasarlas a la vista
            var companyDtos = companies.Select(c => new CreateCompanyDto
            {
                Name = c.Name,
                GroupId = c.GroupId,
                GroupName = c.Group.Name  // Obtener el nombre del grupo asociado
            }).ToList();

            return View("Index", companyDtos);  // Pasa los DTOs a la vista
        }
    }
}
