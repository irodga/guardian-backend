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
            var secrets = _context.Secrets.ToList();
            return View(secrets);
        }

        // GET: /Secrets/Create
        [HttpGet("create")]
        public IActionResult Create()
        {
            var companies = _context.Companies.ToList();
            var model = new CreateSecretDto
            {
                Companies = companies
            };
            return View(model);
        }

        // POST: /Secrets/Create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateSecretDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var vaultPath = $"grupo{dto.CompanyId}/empresa{dto.CompanyId}/{dto.Name.ToLower().Replace(" ", "-")}";
            bool vaultSuccess = false;

            if (dto.Type == "password")
            {
                vaultSuccess = await _vaultKvService.WriteSecretAsync(vaultPath, dto.Value!);
            }
            else if (dto.Type == "fiel" && dto.Files != null)
            {
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

            TempData["LoginMessage"] = "¡Secreto creado correctamente!";
            return RedirectToAction("Index", "Secrets");
        }
    }
}
