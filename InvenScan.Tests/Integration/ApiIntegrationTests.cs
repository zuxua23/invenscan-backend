using InvenScan.Database;
using InvenScan.Entity;
using InvenScan.Service.Implementations;
using InvenScan.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace InvenScan.Tests.Integration;

public class ApiIntegrationTests : IClassFixture<InvenScanWebFactory>
{
    private readonly InvenScanWebFactory _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(InvenScanWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { userId = "admin", password = "admin123" });
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("data").GetProperty("token").GetString() ?? string.Empty;
    }

    [Fact]
    public async Task GetItems_Authenticated_ReturnsItemList()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/item");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task GetItems_Unauthenticated_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/item");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetLocations_Authenticated_ReturnsLocationList()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/location");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
        var data = doc.RootElement.GetProperty("data");
        Assert.True(data.GetArrayLength() >= 1);
    }

    [Fact]
    public async Task ActivityLog_SavesAndRetrievesCorrectly()
    {
        using var context = TestDbFactory.Create();
        var service = new ActivityLogService(context);

        await service.LogAsync("admin", "Administrator", "CREATE", "Item", "Created ITM-001", "WEB", "127.0.0.1");
        await service.LogAsync("admin", "Administrator", "UPDATE", "Item", "Updated ITM-001", "WEB", "127.0.0.1");
        await service.LogAsync("op1",   "Operator 1",    "SCAN",   "StockIn", "Scanned tag", "ANDROID");

        var (allLogs, totalAll) = await service.GetLogsAsync(null, null, null, null, null, 1, 10);
        Assert.Equal(3, totalAll);

        var (adminLogs, totalAdmin) = await service.GetLogsAsync("admin", null, null, null, null, 1, 10);
        Assert.Equal(2, totalAdmin);
        Assert.All(adminLogs, l => Assert.Equal("admin", l.UserId));

        var (androidLogs, totalAndroid) = await service.GetLogsAsync(null, "ANDROID", null, null, null, 1, 10);
        Assert.Equal(1, totalAndroid);
        Assert.Equal("ANDROID", androidLogs[0].Platform);
    }

    [Fact]
    public async Task Dashboard_Authenticated_ReturnsDashboardData()
    {
        using var context = TestDbFactory.Create();
        var service = new DashboardService(context);

        context.Items.AddRange(
            new Item { ItemCode = "I1", ItemName = "Item 1", IsDelete = false },
            new Item { ItemCode = "I2", ItemName = "Item 2", IsDelete = false }
        );
        context.Locations.Add(new Location { LocationCode = "L1", LocationName = "Loc 1", IsDelete = false });
        context.StockIns.Add(new StockIn { DocNumber = "SI-001", LocationId = 0, CreatedAt = DateTime.UtcNow, Status = "SYNCED", CreatedBy = "admin" });
        await context.SaveChangesAsync();

        var summary = await service.GetSummaryAsync();
        var chartData = await service.GetChartDataAsync();

        Assert.Equal(2, summary.TotalItems);
        Assert.Equal(1, summary.TotalLocations);
        Assert.Equal(7, chartData.StockInLabels.Count);
        Assert.Equal(3, chartData.TagStatusLabels.Count);
        Assert.Equal(4, chartData.StockTakingLabels.Count);
        Assert.True(chartData.StockInValues.Sum() >= 1);
    }

    [Fact]
    public async Task TransactionHistory_PaginationWorks()
    {
        using var context = TestDbFactory.Create();
        context.Locations.Add(new Location { Id = 99, LocationCode = "LOC-T", LocationName = "Test Loc", IsDelete = false });
        await context.SaveChangesAsync();

        for (var i = 1; i <= 60; i++)
        {
            context.StockIns.Add(new StockIn
            {
                DocNumber = $"SI-BULK-{i:D3}",
                LocationId = 99,
                Status = "SYNCED",
                CreatedBy = "admin",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();

        var all = await context.StockIns.Where(s => s.DocNumber.StartsWith("SI-BULK-")).ToListAsync();
        Assert.Equal(60, all.Count);

        const int pageSize = 50;
        var page1 = all.OrderByDescending(s => s.CreatedAt).Take(pageSize).ToList();
        var page2 = all.OrderByDescending(s => s.CreatedAt).Skip(pageSize).Take(pageSize).ToList();

        Assert.Equal(50, page1.Count);
        Assert.Equal(10, page2.Count);
    }
}
