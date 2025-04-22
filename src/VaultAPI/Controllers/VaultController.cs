using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("vault")]
public class VaultController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok("Vault backend activo.");
}