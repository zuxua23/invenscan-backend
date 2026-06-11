using InvenScan.Entity;

namespace InvenScan.Service.Interfaces;

public interface IActivityLogService
{
    Task LogAsync(string userId, string userName, string action, string module, string description,
        string platform = "WEB", string? ipAddress = null, string? deviceInfo = null);

    Task<(List<ActivityLog> Items, int Total)> GetLogsAsync(
        string? userId, string? platform, string? module,
        DateTime? from, DateTime? to, int page, int pageSize);

    Task<int> CleanupOldLogsAsync(int olderThanDays);
    Task<int> GetAutoDeleteDaysAsync();
    Task SetAutoDeleteDaysAsync(int days);
}
