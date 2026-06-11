using InvenScan.Database;
using InvenScan.DTO.Request;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Controllers.Web;

[Route("locations")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie)]
public class LocationWebController : Controller
{
    private readonly ILocationService _locationService;
    private readonly IActivityLogService _activityLogService;
    private readonly AppDbContext _context;

    public LocationWebController(ILocationService locationService, IActivityLogService activityLogService, AppDbContext context)
    {
        _locationService = locationService;
        _activityLogService = activityLogService;
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var result = await _locationService.GetAllLocationsAsync();
        return View(result.Data ?? new());
    }

    [HttpGet("create")]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public IActionResult Create() => RedirectToAction(nameof(Index));

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Create(LocationCreateRequest request)
    {
        var isAjax = Request.Headers.ContainsKey("X-Requested-With");

        if (string.IsNullOrWhiteSpace(request.LocationCode) || string.IsNullOrWhiteSpace(request.LocationName))
        {
            if (isAjax) return Json(new { success = false, message = "Location code and name are required." });
            TempData["Error"] = "Location code and name are required.";
            return RedirectToAction(nameof(Index));
        }

        var exists = await _context.Locations.AnyAsync(l => l.LocationCode == request.LocationCode && !l.IsDelete);
        if (exists)
        {
            if (isAjax) return Json(new { success = false, message = "Location code already exists." });
            TempData["Error"] = "Location code already exists.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.Identity?.Name ?? "admin";
        await _context.Locations.AddAsync(new Entity.Location
        {
            LocationCode = request.LocationCode,
            LocationName = request.LocationName,
            Description = request.Description ?? string.Empty,
            CreatedBy = userId
        });
        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(userId, userId, "CREATE", "Locations",
            $"Created location {request.LocationCode} - {request.LocationName}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        if (isAjax) return Json(new { success = true, message = "Location created successfully." });
        TempData["Success"] = "Location created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Edit(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null || location.IsDelete) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Edit(int id, LocationCreateRequest request)
    {
        var isAjax = Request.Headers.ContainsKey("X-Requested-With");
        var location = await _context.Locations.FindAsync(id);

        if (location == null || location.IsDelete)
        {
            if (isAjax) return Json(new { success = false, message = "Location not found." });
            TempData["Error"] = "Location not found.";
            return RedirectToAction(nameof(Index));
        }

        var codeExists = await _context.Locations.AnyAsync(l => l.LocationCode == request.LocationCode && l.Id != id && !l.IsDelete);
        if (codeExists)
        {
            if (isAjax) return Json(new { success = false, message = "Location code already used." });
            TempData["Error"] = "Location code already used.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.Identity?.Name ?? "admin";
        location.LocationCode = request.LocationCode;
        location.LocationName = request.LocationName;
        location.Description = request.Description ?? string.Empty;
        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(userId, userId, "UPDATE", "Locations",
            $"Updated location {request.LocationCode} - {request.LocationName}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        if (isAjax) return Json(new { success = true, message = "Location updated successfully." });
        TempData["Success"] = "Location updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        var isAjax = Request.Headers.ContainsKey("X-Requested-With");
        var location = await _context.Locations.FindAsync(id);

        if (location == null || location.IsDelete)
        {
            if (isAjax) return Json(new { success = false, message = "Location not found." });
            TempData["Error"] = "Location not found.";
            return RedirectToAction(nameof(Index));
        }

        location.IsDelete = true;
        await _context.SaveChangesAsync();

        var userId = User.Identity?.Name ?? "admin";
        await _activityLogService.LogAsync(userId, userId, "DELETE", "Locations",
            $"Deleted location {location.LocationCode} - {location.LocationName}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        if (isAjax) return Json(new { success = true, message = "Location deleted successfully." });
        TempData["Success"] = "Location deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
