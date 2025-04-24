// Ruta: src/VaultAPI/Controllers/SecretsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models.Dto;
using VaultAPI.Models;
using VaultAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Security.Claims;
using Microsoft.Extensions.Logging;  // Asegúrate de tener este namespace para usar ILogger

namespace VaultAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("secrets")]
    public class SecretsController : Controller
    {
        private readonly GuardianDbContext _context;
        private readonly VaultKVService _vaultKvService;
        private readonly ILogger<SecretsController> _logger;  // Inyectar el logger

        public SecretsController(GuardianDbContext context, VaultKVService vaultKvService, ILogger<SecretsController> logger)
        {
            _context = context;
            _vaultKvService = vaultKvService;
            _logger = logger;  // Asignar el logger
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
            _logger.LogInformation("Obteniendo todos los secretos desde la base de datos.");
            var secrets = _context.Secrets.ToList();
            _logger.LogInformation("Se secretos obtenidos: {SecretCount}", secrets.Count);
            return View(secrets);  // Aquí se pasa la lista de secretos a la vista
        }

        // GET: /Secrets/Create
        [HttpGet("create")]
        public IActionResult Create()
        {
            _logger.LogInformation("Cargando empresas y grupos desde la base de datos.");

            // Cargar las empresas y grupos desde la base de datos
            var companies = _context.Companies.ToList();
            var groups = _context.Groups.ToList();

            _logger.LogInformation("Empresas cargadas: {CompanyCount}", companies.Count);
            _logger.LogInformation("Grupos cargados: {GroupCount}", groups.Count);

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
            _logger.LogInformation("Recibido formulario para crear secreto. Datos recibidos:");

            // Log de los datos recibidos
            _logger.LogInformation("Name: {Name}, Type: {Type}, CompanyId: {CompanyId}", dto.Name, dto.Type, dto.CompanyId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("El modelo no es válido. Errores: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(dto);
            }

            // Verificar CompanyId
            if (dto.CompanyId == 0)
            {
                _logger.LogError("CompanyId no ha sido seleccionado o es inválido.");
                ModelState.AddModelError("CompanyId", "Debe seleccionar una empresa.");
                return View(dto);
            }

            _logger.LogInformation("Creando VaultPath...");
            var vaultPath = $"grupo{dto.CompanyId}/empresa{dto.CompanyId}/{dto.Name.ToLower().Replace(" ", "-")}";
            bool vaultSuccess = false;

            // Log del tipo de secreto y acción tomada
            if (dto.Type == "password")
            {
                _logger.LogInformation("Creando secreto de tipo 'password'.");
                vaultSuccess = await _vaultKvService.WriteSecretAsync(vaultPath, dto.Value!);
            }
            else if (dto.Type == "fiel" && dto.Files != null)
            {
                _logger.LogInformation("Creando secreto de tipo 'fiel'.");
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
                _logger.LogError("Error al guardar el secreto en Vault.");
                ModelState.AddModelError("", "Hubo un error al guardar el secreto en Vault.");
                return View(dto);
            }

            _logger.LogInformation("Guardando secreto en la base de datos...");
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

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var secretAccess = new SecretAccess
            {
                UserId = userId,
                SecretId = secret.Id,
                Permission = "read"
            };

            _context.SecretAccesses.Add(secretAccess);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Secreto creado correctamente. Enviando mensaje de éxito.");

            TempData["LoginMessage"] = "¡Secreto creado correctamente!";
            return RedirectToAction("Index", "Secrets");
        }
    }
}
