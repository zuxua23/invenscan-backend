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
    private readonly AppDbContext _context;

    public LocationWebController(ILocationService locationService, AppDbContext context)
    {
        _locationService = locationService;
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
    public IActionResult Create() => View();

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Create(LocationCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LocationCode) || string.IsNullOrWhiteSpace(request.LocationName))
        {
            ViewData["Error"] = "Location code and name are required.";
            return View(request);
        }

        var exists = await _context.Locations.AnyAsync(l => l.LocationCode == request.LocationCode && !l.IsDelete);
        if (exists)
        {
            ViewData["Error"] = "Location code already exists.";
            return View(request);
        }

        var userId = User.Identity?.Name ?? "admin";
        await _context.Locations.AddAsync(new Entity.Location
        {
            LocationCode = request.LocationCode,
            LocationName = request.LocationName,
            Description = request.Description,
            CreatedBy = userId
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "Location created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Edit(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null || location.IsDelete)
            return NotFound();

        return View(location);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Edit(int id, LocationCreateRequest request)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null || location.IsDelete)
            return NotFound();

        var codeExists = await _context.Locations.AnyAsync(l => l.LocationCode == request.LocationCode && l.Id != id && !l.IsDelete);
        if (codeExists)
        {
            ViewData["Error"] = "Location code already used.";
            return View(location);
        }

        location.LocationCode = request.LocationCode;
        location.LocationName = request.LocationName;
        location.Description = request.Description;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Location updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null || location.IsDelete)
            return NotFound();

        location.IsDelete = true;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Location deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
