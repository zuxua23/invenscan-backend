using System.Security.Claims;
using InvenScan.Database;
using InvenScan.DTO.Request;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Controllers.Web;

[Route("stock-prep")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie)]
public class StockPrepWebController : Controller
{
    private readonly IStockPrepService _stockPrepService;
    private readonly IActivityLogService _activityLogService;
    private readonly AppDbContext _context;

    public StockPrepWebController(IStockPrepService stockPrepService, IActivityLogService activityLogService, AppDbContext context)
    {
        _stockPrepService = stockPrepService;
        _activityLogService = activityLogService;
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var result = await _stockPrepService.GetOpenDocumentsAsync();
        ViewBag.Items = await _context.Items.Where(i => !i.IsDelete).OrderBy(i => i.ItemCode).ToListAsync();
        ViewBag.Locations = await _context.Locations.Where(l => !l.IsDelete).OrderBy(l => l.LocationCode).ToListAsync();
        return View(result.Data ?? new());
    }

    [HttpGet("create")]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public IActionResult Create() => RedirectToAction(nameof(Index));

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Create(StockPrepCreateRequest request)
    {
        var isAjax = Request.Headers.ContainsKey("X-Requested-With");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
        var result = await _stockPrepService.CreateAsync(request, userId);

        if (!result.Success)
        {
            if (isAjax) return Json(new { success = false, message = result.Message });
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        await _activityLogService.LogAsync(userId, userId, "CREATE", "StockPrep",
            $"Created picking list {result.Data?.DocNumber}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        if (isAjax) return Json(new { success = true, message = $"Picking list {result.Data?.DocNumber} created." });
        TempData["Success"] = $"Picking list {result.Data?.DocNumber} created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("detail/{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var result = await _stockPrepService.GetByIdAsync(id);
        if (!result.Success) return NotFound();
        return View(result.Data);
    }
}
