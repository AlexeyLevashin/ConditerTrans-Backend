using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
public class PingController : ControllerBase
{
    [HttpGet("ping")]
    public async Task<IActionResult> PingHandler()
    {
        return Ok("pong");
    }
}