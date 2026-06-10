using Microsoft.AspNetCore.Mvc;

namespace InvenScan.Controllers.Api;

[ApiController]
[Route("api/ping")]
public class PingController : ControllerBase
{
    /// <summary>Health check endpoint — no auth required.</summary>
    [HttpGet]
    public IActionResult Ping()
    {
        return Ok(new { success = true, message = "InvenScan API is running.", timestamp = DateTime.UtcNow });
    }
}
