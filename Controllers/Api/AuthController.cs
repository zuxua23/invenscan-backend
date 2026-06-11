using InvenScan.DTO.Request;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace InvenScan.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Authenticate user and return JWT token.</summary>
    [HttpPost("login")]
    [EnableRateLimiting(AppConstants.RateLimitPolicies.Auth)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (!response.Success)
            return Unauthorized(response);

        return Ok(response);
    }
}
