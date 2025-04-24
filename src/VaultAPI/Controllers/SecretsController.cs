// Ruta: src/VaultAPI/Controllers/SecretsController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Models;
using VaultAPI.Models.Dto;  // Asegúrate de importar el namespace correcto para CreateSecretDto
using VaultAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;  // Asegúrate de usar [Authorize]
using System.IO;
using System.Security.Claims;

namespace VaultAPI.Controllers
{
    [Authorize]  // Asegura que solo los usuarios autenticados accedan a este controlador
    [ApiController]
    [Route("secrets")]
    public class SecretsController : Controller
    {
        private readonly GuardianDbContext _context;
        private readonly VaultKVService _vaultKvService;

        // Constructor donde inyectamos GuardianDbContext y VaultKVService
        public SecretsController(GuardianDbContext context, VaultKVService vaultKvService)
        {
            _context = context;
            _vaultKvService = vaultKvService;
        }

        // GET: /Secrets/Index
        [HttpGet("index")]  // Define la ruta explícita para "Index"
        public async Task<IActionResult> Index()
        {
            // Verificar que el usuario esté autenticado
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();  // Devuelve 401 si el usuario no está autenticado
            }

            // Obtener el userId desde los claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();  // Si no se encuentra el userId en los claims, devolver 401
            }

            // Ahora puedes realizar la consulta con userId
            var secrets = await _context.Secrets
                .Where(s => _context.SecretAccesses.Any(sa => sa.UserId == userId && sa.SecretId == s.Id))
                .Include(s => s.Company)
                .ToListAsync();

            if (!secrets.Any())
            {
                return Forbid();  // Si el usuario no tiene permisos, devolver 403
            }

            return View(secrets);
        }

        // GET: /Secrets/Create
        [HttpGet("create")]  // Define una ruta explícita para "Create"
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Secrets/Create
        [HttpPost("create")]  // Define una ruta explícita para el método POST de "Create"
        public async Task<IActionResult> Create(VaultAPI.Models.Dto.CreateSecretDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            // Generar VaultPath
            var vaultPath = $"grupo{dto.CompanyId}/empresa{dto.CompanyId}/{dto.Name.ToLower().Replace(" ", "-")}";
            bool vaultSuccess = false;

            // Lógica para guardar el secreto en Vault
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

                vaultSuccess = await _vaultKvService.WriteSecretRawAsync(vaultPath, fileData);  // Usamos el método WriteSecretRawAsync
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
            var userId = int.Parse(User.Identity.Name!);  // Asegúrate de obtener el userId de la sesión
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
        [HttpGet("view/{id:int}")]  // Define una ruta explícita para "View"
        public async Task<IActionResult> View(int id)
        {
            var secret = await _context.Secrets.FindAsync(id);
            if (secret == null)
                return NotFound();

            var secretValue = await _vaultKvService.ReadSecretAsync(secret.VaultPath);
            return View(secretValue);
        }

        // POST: /Secrets/Delete/{id}
        [HttpDelete("delete/{id:int}")]  // Define una ruta explícita para "Delete"
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
