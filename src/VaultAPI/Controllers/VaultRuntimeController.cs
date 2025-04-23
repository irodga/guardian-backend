// src/VaultAPI/Controllers/VaultRuntimeController.cs
using Microsoft.AspNetCore.Mvc;
using VaultAPI.Services;

namespace VaultAPI.Controllers
{
    [ApiController]
    [Route("vault")]
    public class VaultRuntimeController : ControllerBase
    {
        private readonly string _vaultAddress = "https://api.secrets-guardian.online";

        private async Task<VaultKVService> GetVaultKVServiceAsync()
        {
            var auth = new VaultIamAuthService();
            var token = await auth.LoginAsync();
            return new VaultKVService(_vaultAddress, token!);
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

        [HttpDelete("secret/{*path}/version/{version:int}")]
        public async Task<IActionResult> DeleteSecretVersion(string path, int version)
        {
            var kv = await GetVaultKVServiceAsync();
            var success = await kv.DeleteSecretVersionAsync(path, version);
            return success ? Ok(new { path, version, status = "deleted" }) : BadRequest("Failed to delete version.");
        }
    }
}
