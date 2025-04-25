// Ruta: src/VaultAPI/Controllers/SecretsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using VaultAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace VaultAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("secrets")]
    public class SecretsController : Controller
    {
        private readonly GuardianDbContext _context;
        private readonly VaultKVService _vaultKvService;  // Usamos el VaultKVService para interactuar con Vault
        private readonly ILogger<SecretsController> _logger;

        public SecretsController(GuardianDbContext context, VaultKVService vaultKvService, ILogger<SecretsController> logger)
        {
            _context = context;
            _vaultKvService = vaultKvService;  // Inyección del servicio de Vault
            _logger = logger;
        }

        // Acción para manejar la ruta raíz /Secrets y redirigir a /Secrets/Index
        [HttpGet("")]
        public IActionResult IndexRedirect()
        {
            _logger.LogInformation("Redirigiendo a la página de índice de secretos.");
            return RedirectToAction("Index");
        }

        // GET: /Secrets/Index
        [HttpGet("index")]
        public IActionResult Index()
        {
            _logger.LogInformation("Obteniendo todos los secretos desde la base de datos.");
            var secrets = _context.Secrets.ToList();
            _logger.LogInformation("Cantidad de secretos obtenidos: {SecretCount}", secrets.Count);
            return View(secrets);  // Aquí se pasa la lista de secretos a la vista
        }

        // GET: /Secrets/Create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();  // Simplemente muestra la vista sin ningún modelo adicional
        }

        // POST: /Secrets/Create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] Secret dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("El modelo no es válido. Errores: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(dto);
            }

            // Generar VaultPath dinámicamente basado en la información del formulario
            var vaultPath = $"grupo{dto.CompanyId}/empresa{dto.CompanyId}/{dto.Name.ToLower().Replace(" ", "-")}";
            bool vaultSuccess = false;

            // Llamar a Vault para guardar el secreto
            if (dto.Type == "password")
            {
                // Escribir el secreto en Vault para el tipo 'password'
                vaultSuccess = await _vaultKvService.WriteSecretAsync(vaultPath, dto.Name);
            }
            else if (dto.Type == "fiel")
            {
                // Manejar archivos tipo "fiel" si es necesario (solo ejemplo)
                vaultSuccess = await _vaultKvService.WriteSecretRawAsync(vaultPath, new { data = dto.Name });
            }

            if (!vaultSuccess)
            {
                _logger.LogError("Error al guardar el secreto en Vault.");
                ModelState.AddModelError("", "Hubo un error al guardar el secreto en Vault.");
                return View(dto);
            }

            // Guardar en la base de datos
            var secret = new Secret
            {
                Name = dto.Name,
                Type = dto.Type,
                VaultPath = vaultPath,  // Guardamos el VaultPath, no el valor del secreto
                Expiration = dto.Expiration,
                RequiresApproval = dto.RequiresApproval,
                CompanyId = dto.CompanyId
            };

            _context.Secrets.Add(secret);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Secreto creado correctamente.");
            TempData["LoginMessage"] = "¡Secreto creado correctamente!";
            return RedirectToAction("Index", "Secrets");
        }

        // GET: /Secrets/View/{id}
        [HttpGet("view/{id}")]
        public async Task<IActionResult> ViewSecret(int id)
        {
            // Buscar el secreto en la base de datos usando el ID
            var secret = await _context.Secrets
                .FirstOrDefaultAsync(s => s.Id == id);

            if (secret == null)
            {
                return NotFound();  // Si no se encuentra el secreto, devolver 404
            }

            // Log para verificar el vaultPath que estamos utilizando
            _logger.LogInformation("VaultPath del secreto: {VaultPath}", secret.VaultPath);

            // Llamar al método ReadSecretAsync del servicio VaultKVService para obtener el valor del secreto desde Vault
            var secretValue = await _vaultKvService.ReadSecretAsync(secret.VaultPath);  // Usamos el método de VaultKVService

            if (secretValue == null)
            {
                _logger.LogError("No se pudo recuperar el valor del secreto desde Vault.");
                return View(secret);  // Si no se obtiene el valor del secreto, devolver la vista sin él
            }

            // Pasar el valor recuperado de Vault directamente a la vista usando ViewData
            ViewData["SecretValue"] = secretValue;  // Usamos `ViewData` para pasar el valor del secreto a la vista

            return View(secret);
        }
    }
}
