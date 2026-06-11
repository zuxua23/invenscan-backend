using InvenScan.DTO.Request;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenScan.Controllers.Web;

[Route("users")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
public class UserWebController : Controller
{
    private readonly IUserService _userService;
    private readonly IActivityLogService _activityLogService;

    public UserWebController(IUserService userService, IActivityLogService activityLogService)
    {
        _userService = userService;
        _activityLogService = activityLogService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var result = await _userService.GetAllUsersAsync();
        return View(result.Data ?? new());
    }

    [HttpGet("create")]
    public IActionResult Create() => RedirectToAction(nameof(Index));

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateRequest request)
    {
        var isAjax = Request.Headers.ContainsKey("X-Requested-With");
        var result = await _userService.CreateUserAsync(request);

        if (!result.Success)
        {
            if (isAjax) return Json(new { success = false, message = result.Message });
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        var userId = User.Identity?.Name ?? "admin";
        await _activityLogService.LogAsync(userId, userId, "CREATE", "Users",
            $"Created user {request.UserId}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        if (isAjax) return Json(new { success = true, message = "User created successfully." });
        TempData["Success"] = "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{userId}")]
    public async Task<IActionResult> Edit(string userId)
    {
        var result = await _userService.GetByUserIdAsync(userId);
        if (!result.Success) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("edit/{userId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string userId, UserUpdateRequest request)
    {
        var isAjax = Request.Headers.ContainsKey("X-Requested-With");
        var result = await _userService.UpdateUserAsync(userId, request);

        if (!result.Success)
        {
            if (isAjax) return Json(new { success = false, message = result.Message });
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        var currentUser = User.Identity?.Name ?? "admin";
        await _activityLogService.LogAsync(currentUser, currentUser, "UPDATE", "Users",
            $"Updated user {userId}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        if (isAjax) return Json(new { success = true, message = "User updated successfully." });
        TempData["Success"] = "User updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
