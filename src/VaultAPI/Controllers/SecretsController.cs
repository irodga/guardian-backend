// Ruta: src/VaultAPI/Controllers/SecretsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models.Dto;
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

        // GET: /Secrets/Create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(); // Muestra la vista de creación del secreto
        }

        // POST: /Secrets/Create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateSecretDto dto)
        {
            // Validar los datos del formulario
            if (dto.CompanyId == 0 || string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.Type))
            {
                return View();  // Si los datos son incorrectos, no haces nada, solo vuelves a la vista
            }

            // Crear el VaultPath dinámicamente basado en la información del formulario
            var vaultPath = $"grupo{dto.CompanyId}/empresa{dto.CompanyId}/{dto.Name.ToLower().Replace(" ", "-")}";
            
            // Llamar a Vault para guardar el secreto
            bool vaultSuccess = false;
            if (dto.Type == "password")
            {
                // Guardar el secreto de tipo "password" en Vault
                vaultSuccess = await _vaultKvService.WriteSecretAsync(vaultPath, dto.Name);  // Usamos solo `dto.Name` como valor
            }
            else if (dto.Type == "fiel")
            {
                // Manejar los secretos tipo "fiel"
                vaultSuccess = await _vaultKvService.WriteSecretRawAsync(vaultPath, new { data = dto.Name });  // Solo ejemplo, ajusta según lo necesites
            }

            if (!vaultSuccess)
            {
                // Si la creación del secreto en Vault falla
                return View();  // Redirigir o mostrar un mensaje de error
            }

            // Guardar solo el VaultPath en la base de datos
            var secret = new Secret
            {
                Name = dto.Name,
                Type = dto.Type,
                VaultPath = vaultPath,  // Guardamos solo la ruta de Vault en la base de datos
                Expiration = dto.Expiration,
                RequiresApproval = dto.RequiresApproval,
                CompanyId = dto.CompanyId
            };

            // Añadir el secreto a la base de datos
            _context.Secrets.Add(secret);
            await _context.SaveChangesAsync();

            // Redirigir a la página de índice de secretos
            return RedirectToAction("Index", "Secrets");
        }
    }
}
