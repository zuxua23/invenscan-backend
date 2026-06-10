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
    private readonly AppDbContext _context;

    public ItemWebController(IItemService itemService, AppDbContext context)
    {
        _itemService = itemService;
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
    public IActionResult Create() => View();

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Create(ItemCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ItemCode) || string.IsNullOrWhiteSpace(request.ItemName))
        {
            ViewData["Error"] = "Item code and name are required.";
            return View(request);
        }

        var codeExists = await _context.Items.AnyAsync(i => i.ItemCode == request.ItemCode && !i.IsDelete);
        if (codeExists)
        {
            ViewData["Error"] = "Item code already exists.";
            return View(request);
        }

        var userId = User.Identity?.Name ?? "admin";
        await _context.Items.AddAsync(new Entity.Item
        {
            ItemCode = request.ItemCode,
            ItemName = request.ItemName,
            Description = request.Description,
            Unit = request.Unit,
            MinStock = request.MinStock,
            CreatedBy = userId
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "Item created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null || item.IsDelete)
            return NotFound();

        return View(item);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Edit(int id, ItemCreateRequest request)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null || item.IsDelete)
            return NotFound();

        var codeExists = await _context.Items.AnyAsync(i => i.ItemCode == request.ItemCode && i.Id != id && !i.IsDelete);
        if (codeExists)
        {
            ViewData["Error"] = "Item code already used by another item.";
            return View(item);
        }

        item.ItemCode = request.ItemCode;
        item.ItemName = request.ItemName;
        item.Description = request.Description;
        item.Unit = request.Unit;
        item.MinStock = request.MinStock;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Item updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null || item.IsDelete)
            return NotFound();

        item.IsDelete = true;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Item deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
