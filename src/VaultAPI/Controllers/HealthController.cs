using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("")]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("Guardian backend is up!");
}
