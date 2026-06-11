using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace InvenScan.Controllers.Web;

[Route("activity-log")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Cookie, Roles = "ADMIN")]
public class ActivityLogWebController : Controller
{
    private readonly IActivityLogService _activityLogService;

    public ActivityLogWebController(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? userId, string? platform, string? module,
        string? from, string? to, int page = 1)
    {
        const int pageSize = 50;
        DateTime? fromDate = DateTime.TryParse(from, out var fd) ? fd : null;
        DateTime? toDate = DateTime.TryParse(to, out var td) ? td : null;

        var (logs, total) = await _activityLogService.GetLogsAsync(
            userId, platform, module, fromDate, toDate, page, pageSize);

        var autoDeleteDays = await _activityLogService.GetAutoDeleteDaysAsync();

        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        ViewBag.AutoDeleteDays = autoDeleteDays;
        ViewBag.FilterUserId = userId;
        ViewBag.FilterPlatform = platform;
        ViewBag.FilterModule = module;
        ViewBag.FilterFrom = from;
        ViewBag.FilterTo = to;

        return View(logs);
    }

    [HttpPost("settings")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(int autoDeleteDays)
    {
        if (autoDeleteDays < 1 || autoDeleteDays > 9999)
        {
            TempData["Error"] = "Auto delete days must be between 1 and 9999.";
            return RedirectToAction(nameof(Index));
        }

        await _activityLogService.SetAutoDeleteDaysAsync(autoDeleteDays);
        TempData["Success"] = "Settings saved successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv(
        string? userId, string? platform, string? module,
        string? from, string? to)
    {
        DateTime? fromDate = DateTime.TryParse(from, out var fd) ? fd : null;
        DateTime? toDate = DateTime.TryParse(to, out var td) ? td : null;

        var (logs, _) = await _activityLogService.GetLogsAsync(
            userId, platform, module, fromDate, toDate, 1, 10000);

        var sb = new StringBuilder();
        sb.AppendLine("\"ID\",\"User ID\",\"User Name\",\"Action\",\"Module\",\"Description\",\"Platform\",\"IP Address\",\"Date\"");

        foreach (var log in logs)
        {
            sb.AppendLine($"\"{log.Id}\",\"{log.UserId}\",\"{log.UserName}\",\"{log.Action}\"," +
                          $"\"{log.Module}\",\"{log.Description.Replace("\"", "\"\"")}\",\"{log.Platform}\"," +
                          $"\"{log.IpAddress}\",\"{log.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"activity-log-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
