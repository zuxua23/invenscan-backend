using System.Security.Claims;
using InvenScan.DTO.Request;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenScan.Controllers.Web;

[Route("stock-taking")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie)]
public class StockTakingWebController : Controller
{
    private readonly IStockTakingService _stockTakingService;
    private readonly ILocationService _locationService;
    private readonly IActivityLogService _activityLogService;

    public StockTakingWebController(IStockTakingService stockTakingService, ILocationService locationService, IActivityLogService activityLogService)
    {
        _stockTakingService = stockTakingService;
        _locationService = locationService;
        _activityLogService = activityLogService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var result = await _stockTakingService.GetAllSessionsAsync();
        var locationsResult = await _locationService.GetAllLocationsAsync();
        ViewBag.Locations = locationsResult.Data ?? new();
        return View(result.Data ?? new());
    }

    [HttpGet("detail/{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var sessionResult = await _stockTakingService.GetAllSessionsAsync();
        var session = sessionResult.Data?.FirstOrDefault(s => s.Id == id);
        if (session == null) return NotFound();

        var tagsResult = await _stockTakingService.GetSessionTagsAsync(id);
        ViewData["Session"] = session;
        return View(tagsResult.Data ?? new());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Create(int locationId, string remark)
    {
        var isAjax = Request.Headers.ContainsKey("X-Requested-With");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
        var result = await _stockTakingService.CreateSessionAsync(
            new StockTakingCreateRequest { LocationId = locationId, Remark = remark ?? "" }, userId);

        if (!result.Success)
        {
            if (isAjax) return Json(new { success = false, message = result.Message });
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        await _activityLogService.LogAsync(userId, userId, "CREATE", "StockTaking",
            $"Created stock taking session with remark: {remark}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        if (isAjax) return Json(new { success = true, message = "Stock taking session created." });
        TempData["Success"] = "Stock taking session created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("close/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Close(int id)
    {
        var isAjax = Request.Headers.ContainsKey("X-Requested-With");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
        var result = await _stockTakingService.CloseSessionAsync(id, userId);

        await _activityLogService.LogAsync(userId, userId, result.Success ? "CLOSE" : "CLOSE_FAILED",
            "StockTaking", $"Close session #{id}: {result.Message}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        if (isAjax) return Json(new { success = result.Success, message = result.Message });
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
