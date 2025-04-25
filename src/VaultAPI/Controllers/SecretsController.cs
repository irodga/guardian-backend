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

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(); // Simplemente muestra la vista sin ningún modelo adicional
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateSecretDto dto)
        {
            if (dto.CompanyId == 0 || string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.Type))
            {
                return View();  // Si los datos son incorrectos, no haces nada, solo vuelves a la vista
            }

            // Crear el VaultPath
            var vaultPath = $"grupo{dto.CompanyId}/empresa{dto.CompanyId}/{dto.Name.ToLower().Replace(" ", "-")}";
            bool vaultSuccess = await _vaultKvService.WriteSecretAsync(vaultPath, dto.Value);

            if (!vaultSuccess)
            {
                // Si la creación del secreto en Vault falla
                return View(); 
            }

            // Guardar el secreto en la base de datos
            var secret = new Secret
            {
                Name = dto.Name,
                Type = dto.Type,
                VaultPath = vaultPath,
                Expiration = dto.Expiration,
                CompanyId = dto.CompanyId
            };

            _context.Secrets.Add(secret);
            await _context.SaveChangesAsync();

            // Redirigir a la página de índice (o alguna otra página después de éxito)
            return RedirectToAction("Index", "Secrets");
        }
    }
}