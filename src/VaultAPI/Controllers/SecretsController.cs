// Ruta: src/VaultAPI/Controllers/SecretsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using VaultAPI.Models.Dto;  // Asegúrate de importar el namespace correcto para CreateSecretDto
using VaultAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;  // Asegúrate de usar [Authorize]

namespace VaultAPI.Controllers
{
    [Authorize]  // Asegura que solo los usuarios autenticados accedan a este controlador
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

        // GET: /Secrets
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.Identity!.Name!); // Asegúrate de que el userId esté correctamente configurado

            var secrets = await _context.Secrets
                .Where(s => _context.SecretAccesses.Any(sa => sa.UserId == userId && sa.SecretId == s.Id))
                .Include(s => s.Company)
                .ToListAsync();

            return View(secrets);
        }

        // GET: /Secrets/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Secrets/Create
        [HttpPost]
        public async Task<IActionResult> Create(VaultAPI.Models.Dto.CreateSecretDto dto)  // Usamos el CreateSecretDto correcto
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            // Generar VaultPath
            var vaultPath = $"grupo{dto.CompanyId}/empresa{dto.CompanyId}/{dto.Name.ToLower().Replace(" ", "-")}";

            // Crear en Vault
            bool vaultSuccess = false;
            if (dto.Type == "password")
            {
                vaultSuccess = await _vaultKvService.WriteSecretAsync(vaultPath, dto.Value!);
            }
            else if (dto.Type == "fiel" && dto.Files != null)
            {
                // Convertir archivo a base64 para fiel
                var base64File = Convert.ToBase64String(await File.ReadAllBytesAsync(dto.Files[0].FilePath));
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
            var userId = int.Parse(User.Identity!.Name!);  // Asegúrate de obtener el userId de la sesión
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

        // GET: /Secrets/View/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> View(int id)
        {
            var secret = await _context.Secrets.FindAsync(id);
            if (secret == null)
                return NotFound();

            var secretValue = await _vaultKvService.ReadSecretAsync(secret.VaultPath);
            return View(secretValue);
        }

        // POST: /Secrets/Delete/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var secret = await _context.Secrets.FindAsync(id);
            if (secret == null)
                return NotFound();

            _context.Secrets.Remove(secret);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
