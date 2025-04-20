using Microsoft.AspNetCore.Mvc;

namespace VaultAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VaultController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Vault backend está vivo 👋");
        }
    }
}
