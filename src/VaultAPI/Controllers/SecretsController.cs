// src/VaultAPI/Controllers/SecretsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models.Dto;  // Usamos el DTO correcto para CreateSecretDto
using VaultAPI.Models;
using VaultAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Security.Claims;

namespace VaultAPI.Controllers
{
    [Authorize]  // Solo usuarios autenticados pueden acceder
    [ApiController]
    [Route("secrets")]  // Ruta base para el controlador
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
            // Cargar las empresas y grupos desde la base de datos
            var companies = _context.Companies.ToList();
            var groups = _context.Groups.ToList();

            // Crear el modelo para pasarlo a la vista
            var model = new CreateSecretViewModel
            {
                Companies = companies,
                Groups = groups
            };

            return View(model);  // Pasamos CreateSecretViewModel a la vista
        }

        // POST: /Secrets/Create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateSecretDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);  // Si no es válido, regresamos el formulario con los errores
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
