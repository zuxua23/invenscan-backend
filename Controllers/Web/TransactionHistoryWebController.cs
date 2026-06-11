using InvenScan.Database;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace InvenScan.Controllers.Web;

[Route("transactions")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie)]
public class TransactionHistoryWebController : Controller
{
    private readonly AppDbContext _context;

    public TransactionHistoryWebController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? type, int? locationId, string? status,
        string? from, string? to, int page = 1)
    {
        const int pageSize = 50;
        DateTime? fromDate = DateTime.TryParse(from, out var fd) ? fd : null;
        DateTime? toDate = DateTime.TryParse(to, out var td) ? td : null;

        var transactions = await BuildTransactionListAsync(type, locationId, status, fromDate, toDate);
        var total = transactions.Count;
        var paged = transactions
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var locations = await _context.Locations
            .Where(l => !l.IsDelete)
            .OrderBy(l => l.LocationName)
            .Select(l => new { l.Id, l.LocationCode, l.LocationName })
            .ToListAsync();

        ViewBag.Locations = locations;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        ViewBag.FilterType = type;
        ViewBag.FilterLocationId = locationId;
        ViewBag.FilterStatus = status;
        ViewBag.FilterFrom = from;
        ViewBag.FilterTo = to;

        return View(paged);
    }

    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv(
        string? type, int? locationId, string? status,
        string? from, string? to)
    {
        DateTime? fromDate = DateTime.TryParse(from, out var fd) ? fd : null;
        DateTime? toDate = DateTime.TryParse(to, out var td) ? td : null;

        var transactions = await BuildTransactionListAsync(type, locationId, status, fromDate, toDate);

        var sb = new StringBuilder();
        sb.AppendLine("\"Doc Number\",\"Type\",\"Location\",\"Status\",\"Items\",\"Created By\",\"Date\"");

        foreach (var t in transactions)
        {
            sb.AppendLine($"\"{t.DocNumber}\",\"{t.Type}\",\"{t.LocationName}\"," +
                          $"\"{t.Status}\",\"{t.ItemCount}\",\"{t.CreatedBy}\",\"{t.CreatedAt:yyyy-MM-dd HH:mm}\"");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"transactions-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private async Task<List<TransactionHistoryItem>> BuildTransactionListAsync(
        string? type, int? locationId, string? status,
        DateTime? from, DateTime? to)
    {
        var result = new List<TransactionHistoryItem>();

        if (string.IsNullOrEmpty(type) || type == "STOCK_IN")
        {
            var stockIns = await _context.StockIns
                .Include(s => s.Location)
                .Include(s => s.Details)
                .Where(s =>
                    (!locationId.HasValue || s.LocationId == locationId) &&
                    (string.IsNullOrEmpty(status) || s.Status == status) &&
                    (!from.HasValue || s.CreatedAt >= from) &&
                    (!to.HasValue || s.CreatedAt <= to.Value.AddDays(1)))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            result.AddRange(stockIns.Select(s => new TransactionHistoryItem
            {
                DocNumber = s.DocNumber,
                Type = "STOCK IN",
                LocationName = s.Location?.LocationName ?? "—",
                Status = s.Status,
                ItemCount = s.Details?.Count ?? 0,
                CreatedBy = s.CreatedBy,
                CreatedAt = s.CreatedAt
            }));
        }

        if (string.IsNullOrEmpty(type) || type == "STOCK_PREP")
        {
            var preps = await _context.StockPreps
                .Include(s => s.Details)
                .Where(s =>
                    (string.IsNullOrEmpty(status) || s.Status == status) &&
                    (!from.HasValue || s.CreatedAt >= from) &&
                    (!to.HasValue || s.CreatedAt <= to.Value.AddDays(1)))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            result.AddRange(preps.Select(s => new TransactionHistoryItem
            {
                DocNumber = s.DocNumber,
                Type = "STOCK PREP",
                LocationName = "—",
                Status = s.Status,
                ItemCount = s.Details?.Count ?? 0,
                CreatedBy = s.CreatedBy,
                CreatedAt = s.CreatedAt
            }));
        }

        if (string.IsNullOrEmpty(type) || type == "STOCK_TAKING")
        {
            var sessions = await _context.StockTakings
                .Include(s => s.Details)
                .Where(s =>
                    (string.IsNullOrEmpty(status) || s.Status == status) &&
                    (!from.HasValue || s.CreatedAt >= from) &&
                    (!to.HasValue || s.CreatedAt <= to.Value.AddDays(1)))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            result.AddRange(sessions.Select(s => new TransactionHistoryItem
            {
                DocNumber = s.SessionCode,
                Type = "STOCK TAKING",
                LocationName = "—",
                Status = s.Status,
                ItemCount = s.Details?.Count ?? 0,
                CreatedBy = s.CreatedBy,
                CreatedAt = s.CreatedAt
            }));
        }

        return result.OrderByDescending(r => r.CreatedAt).ToList();
    }
}

public class TransactionHistoryItem
{
    public string DocNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
