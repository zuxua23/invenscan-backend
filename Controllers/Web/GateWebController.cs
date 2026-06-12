using InvenScan.Database;
using InvenScan.Entity;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Controllers.Web;

[Route("gate")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = AppConstants.Roles.Admin)]
public class GateWebController : Controller
{
    private readonly AppDbContext _context;
    private readonly IGateService _gateService;
    private readonly IActivityLogService _activityLog;

    public GateWebController(AppDbContext context, IGateService gateService, IActivityLogService activityLog)
    {
        _context = context;
        _gateService = gateService;
        _activityLog = activityLog;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var gates = await _context.GateConfigs
            .Include(g => g.Location)
            .OrderBy(g => g.GateCode)
            .ToListAsync();

        return View(gates);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        await LoadLocationsAsync();
        var model = new GateConfig { ApiKey = _gateService.GenerateApiKey() };
        return View(model);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(GateConfig model)
    {
        if (string.IsNullOrWhiteSpace(model.GateName) || string.IsNullOrWhiteSpace(model.GateCode))
        {
            TempData["Error"] = "Gate Name and Gate Code are required.";
            await LoadLocationsAsync();
            return View(model);
        }

        var duplicate = await _context.GateConfigs.AnyAsync(g => g.GateCode == model.GateCode);
        if (duplicate)
        {
            TempData["Error"] = $"Gate Code '{model.GateCode}' already exists.";
            await LoadLocationsAsync();
            return View(model);
        }

        model.ApiKey = string.IsNullOrWhiteSpace(model.ApiKey)
            ? _gateService.GenerateApiKey()
            : model.ApiKey;
        model.CreatedAt = DateTime.UtcNow;

        _context.GateConfigs.Add(model);
        await _context.SaveChangesAsync();

        var userId = User.Identity?.Name ?? "unknown";
        await _activityLog.LogAsync(userId, userId, "CREATE", "GateMonitor",
            $"Created gate: {model.GateName} ({model.GateCode})",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = $"Gate '{model.GateName}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var gate = await _context.GateConfigs.FindAsync(id);
        if (gate == null) return NotFound();

        await LoadLocationsAsync();
        return View(gate);
    }

    [HttpPost("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, GateConfig model)
    {
        var gate = await _context.GateConfigs.FindAsync(id);
        if (gate == null) return NotFound();

        if (string.IsNullOrWhiteSpace(model.GateName) || string.IsNullOrWhiteSpace(model.GateCode))
        {
            TempData["Error"] = "Gate Name and Gate Code are required.";
            await LoadLocationsAsync();
            return View(gate);
        }

        var duplicate = await _context.GateConfigs
            .AnyAsync(g => g.GateCode == model.GateCode && g.Id != id);
        if (duplicate)
        {
            TempData["Error"] = $"Gate Code '{model.GateCode}' already used by another gate.";
            await LoadLocationsAsync();
            return View(gate);
        }

        gate.GateName = model.GateName;
        gate.GateCode = model.GateCode;
        gate.LocationId = model.LocationId;
        gate.FieldMapping = string.IsNullOrWhiteSpace(model.FieldMapping) ? "{\"epc\":\"epc\"}" : model.FieldMapping;
        gate.IsActive = model.IsActive;

        await _context.SaveChangesAsync();

        var userId = User.Identity?.Name ?? "unknown";
        await _activityLog.LogAsync(userId, userId, "UPDATE", "GateMonitor",
            $"Updated gate: {gate.GateName} ({gate.GateCode})",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = $"Gate '{gate.GateName}' updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var gate = await _context.GateConfigs.FindAsync(id);
        if (gate == null)
            return Request.Headers.ContainsKey("X-Requested-With")
                ? Json(new { success = false, message = "Gate not found." })
                : RedirectToAction(nameof(Index));

        _context.GateConfigs.Remove(gate);
        await _context.SaveChangesAsync();

        var userId = User.Identity?.Name ?? "unknown";
        await _activityLog.LogAsync(userId, userId, "DELETE", "GateMonitor",
            $"Deleted gate: {gate.GateName} ({gate.GateCode})",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        if (Request.Headers.ContainsKey("X-Requested-With"))
            return Json(new { success = true, message = $"Gate '{gate.GateName}' deleted." });

        TempData["Success"] = $"Gate '{gate.GateName}' deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/log")]
    public async Task<IActionResult> Log(int id)
    {
        var gate = await _context.GateConfigs
            .Include(g => g.Location)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (gate == null) return NotFound();

        ViewBag.Gate = gate;
        return View();
    }

    [HttpGet("{id:int}/log/data")]
    public async Task<IActionResult> LogData(int id, string? date)
    {
        DateTime? filterDate = DateTime.TryParse(date, out var d) ? d : null;
        var logs = await _gateService.GetGateLogsAsync(id, filterDate);
        return Json(logs.Select(l => new
        {
            l.Id,
            l.EpcTag,
            l.ItemName,
            l.Status,
            scannedAt = l.ScannedAt.ToString("HH:mm:ss")
        }));
    }

    [HttpPost("log/{logId:int}/void")]
    public async Task<IActionResult> VoidLog(int logId)
    {
        var success = await _gateService.VoidGateLogAsync(logId);
        return Json(new
        {
            success,
            message = success ? "Log voided and tag status reverted." : "Cannot void this log entry."
        });
    }

    [HttpPost("{id:int}/regenerate-key")]
    public async Task<IActionResult> RegenerateKey(int id)
    {
        var gate = await _context.GateConfigs.FindAsync(id);
        if (gate == null)
            return Json(new { success = false, message = "Gate not found." });

        gate.ApiKey = _gateService.GenerateApiKey();
        await _context.SaveChangesAsync();

        return Json(new { success = true, apiKey = gate.ApiKey });
    }

    private async Task LoadLocationsAsync()
    {
        ViewBag.Locations = await _context.Locations
            .Where(l => !l.IsDelete)
            .OrderBy(l => l.LocationName)
            .Select(l => new { l.Id, l.LocationCode, l.LocationName })
            .ToListAsync();
    }
}
