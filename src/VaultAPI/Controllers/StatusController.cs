using Microsoft.AspNetCore.Mvc;
using VaultAPI;

namespace VaultAPI.Controllers
{
    [ApiController]
    [Route("status")]
    public class StatusController : ControllerBase
    {
        private readonly GuardianDbContext _context;

        public StatusController(GuardianDbContext context)
        {
            _context = context;
        }

        [HttpGet("db")]
        public IActionResult CheckDatabase()
        {
            bool connected = _context.Database.CanConnect();
            return Ok(new { database = connected ? "online" : "offline" });
        }
    }
}
