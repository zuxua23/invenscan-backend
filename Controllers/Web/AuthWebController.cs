using System.Security.Claims;
using InvenScan.Database;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Controllers.Web;

[Route("")]
public class AuthWebController : Controller
{
    private readonly AppDbContext _context;
    private readonly IActivityLogService _activityLogService;

    public AuthWebController(AppDbContext context, IActivityLogService activityLogService)
    {
        _context = context;
        _activityLogService = activityLogService;
    }

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "HomeWeb");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string userId, string password, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
        {
            ViewData["Error"] = "Username and password are required.";
            return View();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            await _activityLogService.LogAsync(userId, userId, "LOGIN_FAILED", "Auth",
                $"Failed login attempt for user {userId}",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                deviceInfo: Request.Headers.UserAgent.ToString());
            ViewData["Error"] = "Invalid username or password.";
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, AppConstants.AuthSchemes.Cookie);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(AppConstants.AuthSchemes.Cookie, principal);

        await _activityLogService.LogAsync(user.UserId, user.FullName, "LOGIN", "Auth",
            $"User {user.UserId} logged in successfully",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
            deviceInfo: Request.Headers.UserAgent.ToString());

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "HomeWeb");
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var userName = User.Identity?.Name ?? "unknown";

        await _activityLogService.LogAsync(userId, userName, "LOGOUT", "Auth",
            $"User {userId} logged out",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        await HttpContext.SignOutAsync(AppConstants.AuthSchemes.Cookie);
        return RedirectToAction("Login");
    }
}
