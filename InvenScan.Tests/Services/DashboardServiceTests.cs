using InvenScan.Entity;
using InvenScan.Service.Implementations;
using InvenScan.Tests.Helpers;

namespace InvenScan.Tests.Services;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetChartData_ReturnsSevenDayStockInLabels()
    {
        using var context = TestDbFactory.Create();
        var service = new DashboardService(context);

        context.StockIns.AddRange(
            new StockIn { DocNumber = "SI-001", LocationId = 0, CreatedAt = DateTime.UtcNow.Date.AddDays(-1), Status = "SYNCED", CreatedBy = "admin" },
            new StockIn { DocNumber = "SI-002", LocationId = 0, CreatedAt = DateTime.UtcNow.Date, Status = "SYNCED", CreatedBy = "admin" }
        );
        await context.SaveChangesAsync();

        var result = await service.GetChartDataAsync();

        Assert.Equal(7, result.StockInLabels.Count);
        Assert.Equal(7, result.StockInValues.Count);
        Assert.Equal(1, result.StockInValues[5]);
        Assert.Equal(1, result.StockInValues[6]);
    }

    [Fact]
    public async Task GetChartData_ReturnsTagStatusCounts()
    {
        using var context = TestDbFactory.Create();
        var service = new DashboardService(context);

        context.Items.Add(new Item { ItemCode = "ITM-001", ItemName = "Item One", IsDelete = false });
        context.Locations.Add(new Location { LocationCode = "LOC-001", LocationName = "Loc One", IsDelete = false });
        await context.SaveChangesAsync();
        var item = context.Items.First();
        var location = context.Locations.First();

        context.Tags.AddRange(
            new Tag { TagId = "T1", EpcTag = "EPC1", ItemId = item.Id, LocationId = location.Id, Status = "IN_STOCK" },
            new Tag { TagId = "T2", EpcTag = "EPC2", ItemId = item.Id, LocationId = location.Id, Status = "IN_STOCK" },
            new Tag { TagId = "T3", EpcTag = "EPC3", ItemId = item.Id, LocationId = location.Id, Status = "OUT" },
            new Tag { TagId = "T4", EpcTag = "EPC4", ItemId = item.Id, LocationId = location.Id, Status = "UNKNOWN" }
        );
        await context.SaveChangesAsync();

        var result = await service.GetChartDataAsync();

        Assert.Equal(3, result.TagStatusLabels.Count);
        Assert.Equal("In Stock", result.TagStatusLabels[0]);
        Assert.Equal(2, result.TagStatusValues[0]);
        Assert.Equal(1, result.TagStatusValues[1]);
        Assert.Equal(1, result.TagStatusValues[2]);
    }

    [Fact]
    public async Task GetChartData_ReturnsFourWeekStockTakingLabels()
    {
        using var context = TestDbFactory.Create();
        var service = new DashboardService(context);

        context.StockTakings.AddRange(
            new StockTaking { SessionCode = "STT-001", Status = "CLOSED", CreatedBy = "admin", CreatedAt = DateTime.UtcNow.Date.AddDays(-3) },
            new StockTaking { SessionCode = "STT-002", Status = "OPEN",   CreatedBy = "admin", CreatedAt = DateTime.UtcNow.Date }
        );
        await context.SaveChangesAsync();

        var result = await service.GetChartDataAsync();

        Assert.Equal(4, result.StockTakingLabels.Count);
        Assert.Equal(4, result.StockTakingValues.Count);
        Assert.True(result.StockTakingValues.Sum() >= 2);
    }

    [Fact]
    public async Task GetSummary_CountsActiveEntities()
    {
        using var context = TestDbFactory.Create();
        var service = new DashboardService(context);

        context.Items.AddRange(
            new Item { ItemCode = "I1", ItemName = "Item 1", IsDelete = false },
            new Item { ItemCode = "I2", ItemName = "Item 2", IsDelete = true }
        );
        context.Locations.Add(new Location { LocationCode = "L1", LocationName = "Loc 1", IsDelete = false });
        context.Users.Add(new User { UserId = "u1", FullName = "User 1", PasswordHash = "hash", Role = "OPERATOR", IsActive = true });
        await context.SaveChangesAsync();

        var summary = await service.GetSummaryAsync();

        Assert.Equal(1, summary.TotalItems);
        Assert.Equal(1, summary.TotalLocations);
        Assert.Equal(1, summary.TotalUsers);
    }
}
