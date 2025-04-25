// Ruta: src/VaultAPI/Controllers/SecretsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models.Dto;
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
        private readonly VaultKVService _vaultKvService;
        private readonly ILogger<SecretsController> _logger;

        public SecretsController(GuardianDbContext context, VaultKVService vaultKvService, ILogger<SecretsController> logger)
        {
            _context = context;
            _vaultKvService = vaultKvService;
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

        // Actualización de la ruta: GET: /Secrets/View/{id}
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

            // Llamar al método ReadSecretAsync del servicio VaultKVService para obtener el valor del secreto desde Vault
            var secretValue = await _vaultKvService.ReadSecretAsync(secret.VaultPath);  // Usamos el método de VaultKVService

            if (secretValue == null)
            {
                _logger.LogError("No se pudo recuperar el valor del secreto desde Vault.");
                return View(secret);  // Si no se obtiene el valor del secreto, devolver la vista sin él
            }

            // Asignar el valor recuperado de Vault al modelo del secreto
            secret.Value = secretValue;

            // Pasar el secreto con el valor recuperado a la vista
            return View(secret);
        }
    }
}
