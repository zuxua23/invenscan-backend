using System.Security.Claims;
using InvenScan.Database;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Controllers.Web;

[Route("")]
public class AuthWebController : Controller
{
    private readonly AppDbContext _context;

    public AuthWebController(AppDbContext context)
    {
        _context = context;
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

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "HomeWeb");
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AppConstants.AuthSchemes.Cookie);
        return RedirectToAction("Login");
    }
}
