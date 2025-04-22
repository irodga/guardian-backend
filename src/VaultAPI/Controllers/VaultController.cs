using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("vault")]
public class VaultController : ControllerBase
{
    [HttpGet]
    public IActionResult Index() =>
        Ok(new { message = "Vault API está activa.", test_url = "/vault/test" });

    [HttpGet("ping")]
    public IActionResult Ping() => Ok("Vault backend está vivo 👋");

    [HttpGet("test")]
    public async Task<IActionResult> TestLogin()
    {
        var vaultService = new VaultIamAuthService();
        var token = await vaultService.LoginAsync();

        if (!string.IsNullOrWhiteSpace(token))
            return Ok(new { message = "Token obtenido correctamente", token });

        return StatusCode(500, new { message = "Falló el login IAM con Vault" });
    }
}
