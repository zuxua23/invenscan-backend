using InvenScan.Entity;
using InvenScan.Service.Implementations;
using InvenScan.Tests.Helpers;

namespace InvenScan.Tests.Services;

public class ActivityLogServiceTests
{
    [Fact]
    public async Task Log_ValidAction_SavesLog()
    {
        using var context = TestDbFactory.Create();
        var service = new ActivityLogService(context);

        await service.LogAsync("user1", "John Doe", "CREATE", "Item", "Created item ITM-001", "WEB", "127.0.0.1");

        var logs = context.ActivityLogs.ToList();
        Assert.Single(logs);
        Assert.Equal("user1", logs[0].UserId);
        Assert.Equal("John Doe", logs[0].UserName);
        Assert.Equal("CREATE", logs[0].Action);
        Assert.Equal("Item", logs[0].Module);
        Assert.Equal("Created item ITM-001", logs[0].Description);
        Assert.Equal("WEB", logs[0].Platform);
        Assert.Equal("127.0.0.1", logs[0].IpAddress);
    }

    [Fact]
    public async Task AutoDelete_ExpiredLogs_DeletesCorrectly()
    {
        using var context = TestDbFactory.Create();
        var service = new ActivityLogService(context);

        context.ActivityLogs.AddRange(
            new ActivityLog { UserId = "u1", UserName = "U1", Action = "A", Module = "M", Description = "D", CreatedAt = DateTime.UtcNow.AddDays(-100) },
            new ActivityLog { UserId = "u2", UserName = "U2", Action = "A", Module = "M", Description = "D", CreatedAt = DateTime.UtcNow.AddDays(-50) },
            new ActivityLog { UserId = "u3", UserName = "U3", Action = "A", Module = "M", Description = "D", CreatedAt = DateTime.UtcNow.AddDays(-1) }
        );
        await context.SaveChangesAsync();

        var deleted = await service.CleanupOldLogsAsync(60);

        Assert.Equal(1, deleted);
        Assert.Equal(2, context.ActivityLogs.Count());
    }

    [Fact]
    public async Task GetLogs_WithFilters_ReturnsFiltered()
    {
        using var context = TestDbFactory.Create();
        var service = new ActivityLogService(context);

        context.ActivityLogs.AddRange(
            new ActivityLog { UserId = "admin", UserName = "Admin", Action = "LOGIN",  Module = "Auth", Description = "D", Platform = "WEB", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new ActivityLog { UserId = "admin", UserName = "Admin", Action = "CREATE", Module = "Item", Description = "D", Platform = "WEB", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new ActivityLog { UserId = "op1",   UserName = "Op1",   Action = "SCAN",   Module = "StockIn", Description = "D", Platform = "ANDROID", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var (webLogs, webTotal) = await service.GetLogsAsync("admin", null, null, null, null, 1, 10);
        Assert.Equal(2, webTotal);
        Assert.All(webLogs, l => Assert.Equal("admin", l.UserId));

        var (androidLogs, androidTotal) = await service.GetLogsAsync(null, "ANDROID", null, null, null, 1, 10);
        Assert.Equal(1, androidTotal);
        Assert.Equal("ANDROID", androidLogs[0].Platform);

        var (itemLogs, itemTotal) = await service.GetLogsAsync(null, null, "Item", null, null, 1, 10);
        Assert.Equal(1, itemTotal);
        Assert.Equal("Item", itemLogs[0].Module);
    }

    [Fact]
    public async Task GetLogs_Pagination_ReturnsCorrectPage()
    {
        using var context = TestDbFactory.Create();
        var service = new ActivityLogService(context);

        for (var i = 0; i < 15; i++)
        {
            context.ActivityLogs.Add(new ActivityLog
            {
                UserId = "u", UserName = "U", Action = "A", Module = "M", Description = $"Log {i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();

        var (page1, total) = await service.GetLogsAsync(null, null, null, null, null, 1, 10);
        var (page2, _) = await service.GetLogsAsync(null, null, null, null, null, 2, 10);

        Assert.Equal(15, total);
        Assert.Equal(10, page1.Count);
        Assert.Equal(5, page2.Count);
    }

    [Fact]
    public async Task GetAutoDeleteDays_NoSetting_ReturnsDefault90()
    {
        using var context = TestDbFactory.Create();
        var service = new ActivityLogService(context);

        var days = await service.GetAutoDeleteDaysAsync();

        Assert.Equal(90, days);
    }

    [Fact]
    public async Task SetAutoDeleteDays_NewValue_PersistsCorrectly()
    {
        using var context = TestDbFactory.Create();
        var service = new ActivityLogService(context);

        await service.SetAutoDeleteDaysAsync(30);
        var days = await service.GetAutoDeleteDaysAsync();

        Assert.Equal(30, days);
    }

    [Fact]
    public async Task SetAutoDeleteDays_UpdateExisting_OverwritesValue()
    {
        using var context = TestDbFactory.Create();
        var service = new ActivityLogService(context);

        await service.SetAutoDeleteDaysAsync(30);
        await service.SetAutoDeleteDaysAsync(60);
        var days = await service.GetAutoDeleteDaysAsync();

        Assert.Equal(60, days);
        Assert.Single(context.AppSettings.ToList());
    }
}
