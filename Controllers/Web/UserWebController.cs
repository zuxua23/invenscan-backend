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

    public UserWebController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var result = await _userService.GetAllUsersAsync();
        return View(result.Data ?? new());
    }

    [HttpGet("create")]
    public IActionResult Create() => View();

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        if (!result.Success)
        {
            ViewData["Error"] = result.Message;
            return View(request);
        }

        TempData["Success"] = "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{userId}")]
    public async Task<IActionResult> Edit(string userId)
    {
        var result = await _userService.GetByUserIdAsync(userId);
        if (!result.Success)
            return NotFound();

        return View(result.Data);
    }

    [HttpPost("edit/{userId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string userId, UserUpdateRequest request)
    {
        var result = await _userService.UpdateUserAsync(userId, request);
        if (!result.Success)
        {
            ViewData["Error"] = result.Message;
            return View();
        }

        TempData["Success"] = "User updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
