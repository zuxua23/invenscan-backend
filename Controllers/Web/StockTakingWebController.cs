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

    public StockTakingWebController(IStockTakingService stockTakingService)
    {
        _stockTakingService = stockTakingService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var result = await _stockTakingService.GetAllSessionsAsync();
        return View(result.Data ?? new());
    }

    [HttpGet("detail/{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var sessionResult = await _stockTakingService.GetAllSessionsAsync();
        var session = sessionResult.Data?.FirstOrDefault(s => s.Id == id);
        if (session == null)
            return NotFound();

        var tagsResult = await _stockTakingService.GetSessionTagsAsync(id);
        ViewData["Session"] = session;
        return View(tagsResult.Data ?? new());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Create(string remark)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
        var result = await _stockTakingService.CreateSessionAsync(new StockTakingCreateRequest { Remark = remark ?? "" }, userId);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Stock taking session created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("close/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Close(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin";
        var result = await _stockTakingService.CloseSessionAsync(id, userId);

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
