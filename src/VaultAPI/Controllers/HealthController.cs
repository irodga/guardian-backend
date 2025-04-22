using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("")]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("Guardian backend is up!");
}

[ApiController]
[Route("vault")]
public class VaultController : ControllerBase
{
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
}