// src/VaultAPI/Controllers/VaultRuntimeController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;  // Para usar IOptions


namespace VaultAPI.Controllers
{
    [Authorize] 
    [ApiController]
    [Route("vault")]
     public class VaultRuntimeController : ControllerBase
    {
        // Mantener la dirección de Vault hardcodeada como estaba
        private readonly string _vaultAddress = "https://api.secrets-guardian.online";
        
        private readonly VaultIamAuthService _vaultIamAuthService;

        // Constructor donde inyectamos VaultIamAuthService
        public VaultRuntimeController(VaultIamAuthService vaultIamAuthService)
        {
            _vaultIamAuthService = vaultIamAuthService;  // Guardamos la referencia de VaultIamAuthService
        }

        // Método para obtener el servicio VaultKVService con el token
        private async Task<VaultKVService> GetVaultKVServiceAsync()
        {
            // Usamos el servicio inyectado para obtener el token
            var token = await _vaultIamAuthService.LoginAsync();
            return new VaultKVService(_vaultAddress, token!);  // Pasamos la URL y el token
        }

        [HttpGet("secret/{*path}")]
        public async Task<IActionResult> ReadSecret(string path)
        {
            var kv = await GetVaultKVServiceAsync();
            var value = await kv.ReadSecretAsync(path);
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
            var kv = await GetVaultKVServiceAsync();
            var success = await kv.WriteSecretAsync(path, request.Value, request.Cas);
            return success ? Ok(new { path, status = "written" }) : BadRequest("Failed to write secret.");
        }

        [HttpDelete("secret-version/{*path}")]
        public async Task<IActionResult> DeleteSecretVersion(string path, [FromQuery] int version)
        {
            var kv = await GetVaultKVServiceAsync();
            var success = await kv.DeleteSecretVersionAsync(path, version);
            return success ? Ok(new { path, version, status = "deleted" }) : BadRequest("Failed to delete version.");
        }
    }
}
