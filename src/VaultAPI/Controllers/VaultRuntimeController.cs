// Ruta: src/VaultAPI/Controllers/VaultRuntimeController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;  // Para usar IOptions
using VaultAPI;

namespace VaultAPI.Controllers
{
    [Authorize] 
    [ApiController]
    [Route("vault")]
    public class VaultRuntimeController : ControllerBase
    {
        private readonly VaultIamAuthService _vaultIamAuthService;
        private readonly string _vaultAddress;

        // Constructor donde inyectamos IOptions<VaultConfig> y VaultIamAuthService
        public VaultRuntimeController(IOptions<VaultConfig> vaultConfig, VaultIamAuthService vaultIamAuthService)
        {
            _vaultAddress = vaultConfig.Value.VaultAddress;  // Usamos la URL de Vault desde la configuración
            _vaultIamAuthService = vaultIamAuthService;  // Guardamos la referencia de VaultIamAuthService
        }

        // Método para obtener el servicio VaultKVService con el token
        private async Task<VaultKVService> GetVaultKVServiceAsync()
        {
            var token = await _vaultIamAuthService.LoginAsync();
            return new VaultKVService(_vaultAddress, token!);  // Usamos la URL y el token
        }

        [HttpGet("secret/{*path}")]
        public async Task<IActionResult> ReadSecret(string path)
        {
            var value = await GetVaultKVServiceAsync().ReadSecretAsync(path);
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
            var success = await GetVaultKVServiceAsync().WriteSecretAsync(path, request.Value, request.Cas);
            return success ? Ok(new { path, status = "written" }) : BadRequest("Failed to write secret.");
        }

        [HttpDelete("secret-version/{*path}")]
        public async Task<IActionResult> DeleteSecretVersion(string path, [FromQuery] int version)
        {
            var success = await GetVaultKVServiceAsync().DeleteSecretVersionAsync(path, version);
            return success ? Ok(new { path, version, status = "deleted" }) : BadRequest("Failed to delete version.");
        }
    }
}
