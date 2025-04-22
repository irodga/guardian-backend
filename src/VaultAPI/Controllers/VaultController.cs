using Microsoft.AspNetCore.Mvc;

namespace VaultAPI.Controllers
{
    [ApiController]
    [Route("vault")]
    public class VaultController : ControllerBase
    {
        // 👉 GET /vault
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new
            {
                message = "Vault API está activa.",
                test_url = "/vault/test",
                info = "Usa /vault/test para probar autenticación IAM con Vault."
            });
        }

        // 👉 GET /vault/test
        [HttpGet("test")]
        public async Task<IActionResult> TestLogin()
        {
            var vaultService = new VaultIamAuthService();
            var token = await vaultService.LoginAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                return Ok(new { message = "Token obtenido correctamente", token });
            }

            return StatusCode(500, new { message = "Falló el login IAM con Vault" });
        }

        // 👉 GET /vault/ping
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Vault backend está vivo 👋");
        }
    }
}
