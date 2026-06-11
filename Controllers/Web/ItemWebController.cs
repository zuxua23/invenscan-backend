using InvenScan.Database;
using InvenScan.DTO.Request;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Controllers.Web;

[Route("items")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie)]
public class ItemWebController : Controller
{
    private readonly IItemService _itemService;
    private readonly IActivityLogService _activityLogService;
    private readonly AppDbContext _context;

    public ItemWebController(IItemService itemService, IActivityLogService activityLogService, AppDbContext context)
    {
        _itemService = itemService;
        _activityLogService = activityLogService;
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var result = await _itemService.GetAllItemsAsync();
        return View(result.Data ?? new());
    }

    [HttpGet("create")]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public IActionResult Create() => RedirectToAction(nameof(Index));

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Create(ItemCreateRequest request)
    {
        var isAjax = Request.Headers.ContainsKey("X-Requested-With");

        if (string.IsNullOrWhiteSpace(request.ItemCode) || string.IsNullOrWhiteSpace(request.ItemName))
        {
            if (isAjax) return Json(new { success = false, message = "Item code and name are required." });
            TempData["Error"] = "Item code and name are required.";
            return RedirectToAction(nameof(Index));
        }

        var codeExists = await _context.Items.AnyAsync(i => i.ItemCode == request.ItemCode && !i.IsDelete);
        if (codeExists)
        {
            if (isAjax) return Json(new { success = false, message = "Item code already exists." });
            TempData["Error"] = "Item code already exists.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.Identity?.Name ?? "admin";
        await _context.Items.AddAsync(new Entity.Item
        {
            ItemCode = request.ItemCode,
            ItemName = request.ItemName,
            Description = request.Description ?? string.Empty,
            Unit = request.Unit ?? string.Empty,
            MinStock = request.MinStock,
            CreatedBy = userId
        });
        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(userId, userId, "CREATE", "Items",
            $"Created item {request.ItemCode} - {request.ItemName}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        if (isAjax) return Json(new { success = true, message = "Item created successfully." });
        TempData["Success"] = "Item created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null || item.IsDelete) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Edit(int id, ItemCreateRequest request)
    {
        var isAjax = Request.Headers.ContainsKey("X-Requested-With");
        var item = await _context.Items.FindAsync(id);

        if (item == null || item.IsDelete)
        {
            if (isAjax) return Json(new { success = false, message = "Item not found." });
            TempData["Error"] = "Item not found.";
            return RedirectToAction(nameof(Index));
        }

        var codeExists = await _context.Items.AnyAsync(i => i.ItemCode == request.ItemCode && i.Id != id && !i.IsDelete);
        if (codeExists)
        {
            if (isAjax) return Json(new { success = false, message = "Item code already used by another item." });
            TempData["Error"] = "Item code already used by another item.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.Identity?.Name ?? "admin";
        item.ItemCode = request.ItemCode;
        item.ItemName = request.ItemName;
        item.Description = request.Description ?? string.Empty;
        item.Unit = request.Unit ?? string.Empty;
        item.MinStock = request.MinStock;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(userId, userId, "UPDATE", "Items",
            $"Updated item {request.ItemCode} - {request.ItemName}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        if (isAjax) return Json(new { success = true, message = "Item updated successfully." });
        TempData["Success"] = "Item updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        var isAjax = Request.Headers.ContainsKey("X-Requested-With");
        var item = await _context.Items.FindAsync(id);

        if (item == null || item.IsDelete)
        {
            if (isAjax) return Json(new { success = false, message = "Item not found." });
            TempData["Error"] = "Item not found.";
            return RedirectToAction(nameof(Index));
        }

        item.IsDelete = true;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var userId = User.Identity?.Name ?? "admin";
        await _activityLogService.LogAsync(userId, userId, "DELETE", "Items",
            $"Deleted item {item.ItemCode} - {item.ItemName}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        if (isAjax) return Json(new { success = true, message = "Item deleted successfully." });
        TempData["Success"] = "Item deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
