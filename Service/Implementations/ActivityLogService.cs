using InvenScan.Database;
using InvenScan.Entity;
using InvenScan.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Service.Implementations;

public class ActivityLogService : IActivityLogService
{
    private readonly AppDbContext _context;

    public ActivityLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string userId, string userName, string action, string module,
        string description, string platform = "WEB", string? ipAddress = null, string? deviceInfo = null)
    {
        try
        {
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                Module = module,
                Description = description,
                Platform = platform,
                IpAddress = ipAddress,
                DeviceInfo = deviceInfo,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Logging must never throw
        }
    }

    public async Task<(List<ActivityLog> Items, int Total)> GetLogsAsync(
        string? userId, string? platform, string? module,
        DateTime? from, DateTime? to, int page, int pageSize)
    {
        var query = _context.ActivityLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(l => l.UserId == userId);

        if (!string.IsNullOrWhiteSpace(platform))
            query = query.Where(l => l.Platform == platform);

        if (!string.IsNullOrWhiteSpace(module))
            query = query.Where(l => l.Module == module);

        if (from.HasValue)
            query = query.Where(l => l.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(l => l.CreatedAt <= to.Value.AddDays(1));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<int> CleanupOldLogsAsync(int olderThanDays)
    {
        var cutoff = DateTime.UtcNow.AddDays(-olderThanDays);
        var old = await _context.ActivityLogs.Where(l => l.CreatedAt < cutoff).ToListAsync();
        _context.ActivityLogs.RemoveRange(old);
        await _context.SaveChangesAsync();
        return old.Count;
    }

    public async Task<int> GetAutoDeleteDaysAsync()
    {
        var setting = await _context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == "ActivityLog.AutoDeleteDays");
        return int.TryParse(setting?.Value, out var days) ? days : 90;
    }

    public async Task SetAutoDeleteDaysAsync(int days)
    {
        var setting = await _context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == "ActivityLog.AutoDeleteDays");

        if (setting == null)
        {
            _context.AppSettings.Add(new AppSetting
            {
                Key = "ActivityLog.AutoDeleteDays",
                Value = days.ToString(),
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            setting.Value = days.ToString();
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
