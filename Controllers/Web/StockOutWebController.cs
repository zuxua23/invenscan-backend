using InvenScan.Database;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Controllers.Web;

[Route("stockout")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie)]
public class StockOutWebController : Controller
{
    private readonly AppDbContext _context;
    private readonly IActivityLogService _activityLog;

    public StockOutWebController(AppDbContext context, IActivityLogService activityLog)
    {
        _context = context;
        _activityLog = activityLog;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var stockOuts = await _context.StockOuts
            .Include(s => s.Location)
            .Include(s => s.Details)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return View(stockOuts);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var stockOut = await _context.StockOuts
            .Include(s => s.Location)
            .Include(s => s.Details)
                .ThenInclude(d => d.Item)
            .Include(s => s.Details)
                .ThenInclude(d => d.Tag)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (stockOut == null)
        {
            TempData["Error"] = "Stock out document not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(stockOut);
    }
}
