// Ruta: src/VaultAPI/Controllers/SecretsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models.Dto;  // Importa el namespace correcto para 'CreateSecretDto'
using VaultAPI.Models;
using VaultAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Security.Claims;

namespace VaultAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("secrets")]
    public class SecretsController : Controller
    {
        private readonly GuardianDbContext _context;
        private readonly VaultKVService _vaultKvService;

        public SecretsController(GuardianDbContext context, VaultKVService vaultKvService)
        {
            _context = context;
            _vaultKvService = vaultKvService;
        }

        // Acción para manejar la ruta raíz /Secrets y redirigir a /Secrets/Index
        [HttpGet("")]
        public IActionResult IndexRedirect()
        {
            return RedirectToAction("Index");
        }

        // GET: /Secrets/Index
        [HttpGet("index")]
        public IActionResult Index()
        {
            // Obtener todos los secretos de la base de datos para la vista
            var secrets = _context.Secrets.ToList();
            return View(secrets);  // Aquí se pasa la lista de secretos a la vista
        }

        // GET: /Secrets/Create
        [HttpGet("create")]
        public IActionResult Create()
        {
            // Cargar las empresas y grupos desde la base de datos
            var companies = _context.Companies.ToList();
            var groups = _context.Groups.ToList();

            // Crear el modelo para pasarlo a la vista
            var model = new CreateSecretDto
            {
                Companies = companies,
                Groups = groups
            };

            return View(model);  // Pasamos CreateSecretDto a la vista
        }

        // POST: /Secrets/Create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateSecretDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            // Validación adicional para asegurarnos de que los campos requeridos no estén vacíos
            if (dto.Companies == null || !dto.Companies.Any())
            {
                ModelState.AddModelError("Companies", "Debe seleccionar al menos una empresa.");
            }

            if (dto.Groups == null || !dto.Groups.Any())
            {
                ModelState.AddModelError("Groups", "Debe seleccionar al menos un grupo.");
            }

            if (!ModelState.IsValid)
            {
                return View(dto);  // Regresamos la vista con los errores de validación
            }

            // Generar VaultPath
            var vaultPath = $"grupo{dto.CompanyId}/empresa{dto.CompanyId}/{dto.Name.ToLower().Replace(" ", "-")}";
            bool vaultSuccess = false;

            if (dto.Type == "password")
            {
                vaultSuccess = await _vaultKvService.WriteSecretAsync(vaultPath, dto.Value!);
            }
            else if (dto.Type == "fiel" && dto.Files != null)
            {
                // Manejar archivos tipo "fiel"
                using var memoryStream = new MemoryStream();
                await dto.Files[0].CopyToAsync(memoryStream);
                var base64File = Convert.ToBase64String(memoryStream.ToArray());

                var fileData = new Dictionary<string, object>
                {
                    { "filename", dto.Files[0].FileName },
                    { "data", base64File }
                };

                vaultSuccess = await _vaultKvService.WriteSecretRawAsync(vaultPath, fileData);
            }

            if (!vaultSuccess)
            {
                ModelState.AddModelError("", "Hubo un error al guardar el secreto en Vault.");
                return View(dto);
            }

            // Guardar en la base de datos
            var secret = new Secret
            {
                Name = dto.Name,
                Type = dto.Type,
                VaultPath = vaultPath,
                Expiration = dto.Expiration,
                RequiresApproval = dto.RequiresApproval,
                CompanyId = dto.CompanyId
            };

            _context.Secrets.Add(secret);
            await _context.SaveChangesAsync();

            // Insertar acceso automático para el usuario que lo crea
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var secretAccess = new SecretAccess
            {
                UserId = userId,
                SecretId = secret.Id,
                Permission = "read"
            };

            _context.SecretAccesses.Add(secretAccess);
            await _context.SaveChangesAsync();

            TempData["LoginMessage"] = "¡Secreto creado correctamente!";
            return RedirectToAction("Index", "Secrets");
        }
    }
}
