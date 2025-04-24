// Ruta: src/VaultAPI/Controllers/VaultRuntimeController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;  // Para inyectar IOptions
using VaultAPI;

namespace VaultAPI.Controllers
{
    [Authorize] 
    [ApiController]
    [Route("vault")]
    public class VaultRuntimeController : ControllerBase
    {
        private readonly VaultKVService _vaultKVService;

        // Constructor donde inyectamos IOptions<VaultConfig> y VaultIamAuthService
        public VaultRuntimeController(IOptions<VaultConfig> vaultConfig, VaultIamAuthService vaultIamAuthService)
        {
            // Usamos la configuración de Vault y el servicio de autenticación para crear VaultKVService
            _vaultKVService = new VaultKVService(vaultConfig, vaultIamAuthService);  // Creación de VaultKVService con inyección
        }

        [HttpGet("secret/{*path}")]
        public async Task<IActionResult> ReadSecret(string path)
        {
            var value = await _vaultKVService.ReadSecretAsync(path);  // Usamos VaultKVService inyectado
            if (value == null) return NotFound();
            return Ok(new { path, value });
        }

        public class WriteRequest
        {
            public string Value { get; set; } = string.Empty;
            public int Cas { get; set; } = 0;
        }

        [HttpPost("secret/{*path}")]
        public async Task<IActionResult> WriteSecret(string path, [FromBody] WriteRequest request)
        {
            var success = await _vaultKVService.WriteSecretAsync(path, request.Value, request.Cas);
            return success ? Ok(new { path, status = "written" }) : BadRequest("Failed to write secret.");
        }

        [HttpDelete("secret-version/{*path}")]
        public async Task<IActionResult> DeleteSecretVersion(string path, [FromQuery] int version)
        {
            var success = await _vaultKVService.DeleteSecretVersionAsync(path, version);
            return success ? Ok(new { path, version, status = "deleted" }) : BadRequest("Failed to delete version.");
        }
    }
}
