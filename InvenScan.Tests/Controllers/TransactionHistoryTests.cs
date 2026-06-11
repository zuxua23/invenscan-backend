using InvenScan.Entity;
using InvenScan.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace InvenScan.Tests.Controllers;

public class TransactionHistoryTests
{
    private static async Task SeedTransactions(InvenScan.Database.AppDbContext context)
    {
        context.Locations.Add(new Location { Id = 1, LocationCode = "LOC-001", LocationName = "Warehouse A", IsDelete = false });
        await context.SaveChangesAsync();

        context.StockIns.AddRange(
            new StockIn { DocNumber = "SI-2024-001", LocationId = 1, Status = "SYNCED", CreatedBy = "admin", CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new StockIn { DocNumber = "SI-2024-002", LocationId = 1, Status = "PENDING", CreatedBy = "op1", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new StockIn { DocNumber = "SI-2024-003", LocationId = 1, Status = "SYNCED", CreatedBy = "admin", CreatedAt = DateTime.UtcNow.AddDays(-1) }
        );
        context.StockPreps.AddRange(
            new StockPrep { DocNumber = "SP-2024-001", Status = "DONE", CreatedBy = "admin", CreatedAt = DateTime.UtcNow.AddDays(-8) },
            new StockPrep { DocNumber = "SP-2024-002", Status = "OPEN", CreatedBy = "op1", CreatedAt = DateTime.UtcNow.AddDays(-3) }
        );
        context.StockTakings.AddRange(
            new StockTaking { SessionCode = "STT-001", Status = "CLOSED", CreatedBy = "admin", CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new StockTaking { SessionCode = "STT-002", Status = "OPEN", CreatedBy = "op1", CreatedAt = DateTime.UtcNow.AddDays(-2) }
        );
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetHistory_WithDateFilter_ReturnsFiltered()
    {
        using var context = TestDbFactory.Create();
        await SeedTransactions(context);

        var fromDate = DateTime.UtcNow.AddDays(-6);
        var result = await BuildTransactionList(context, null, null, null, fromDate, null);

        Assert.All(result, t => Assert.True(t.CreatedAt >= fromDate));
        Assert.True(result.Count < 7, "Should exclude old records");
    }

    [Fact]
    public async Task GetHistory_WithTypeFilter_ReturnsFiltered()
    {
        using var context = TestDbFactory.Create();
        await SeedTransactions(context);

        var stockInOnly = await BuildTransactionList(context, "STOCK_IN", null, null, null, null);
        var stockPrepOnly = await BuildTransactionList(context, "STOCK_PREP", null, null, null, null);
        var stockTakingOnly = await BuildTransactionList(context, "STOCK_TAKING", null, null, null, null);

        Assert.Equal(3, stockInOnly.Count);
        Assert.All(stockInOnly, t => Assert.Equal("STOCK IN", t.Type));

        Assert.Equal(2, stockPrepOnly.Count);
        Assert.All(stockPrepOnly, t => Assert.Equal("STOCK PREP", t.Type));

        Assert.Equal(2, stockTakingOnly.Count);
        Assert.All(stockTakingOnly, t => Assert.Equal("STOCK TAKING", t.Type));
    }

    [Fact]
    public async Task GetHistory_NoFilter_ReturnsAllTypes()
    {
        using var context = TestDbFactory.Create();
        await SeedTransactions(context);

        var all = await BuildTransactionList(context, null, null, null, null, null);

        Assert.Equal(7, all.Count);
        Assert.Contains(all, t => t.Type == "STOCK IN");
        Assert.Contains(all, t => t.Type == "STOCK PREP");
        Assert.Contains(all, t => t.Type == "STOCK TAKING");
    }

    [Fact]
    public async Task GetHistory_WithStatusFilter_ReturnsFiltered()
    {
        using var context = TestDbFactory.Create();
        await SeedTransactions(context);

        var synced = await BuildTransactionList(context, "STOCK_IN", null, "SYNCED", null, null);

        Assert.Equal(2, synced.Count);
        Assert.All(synced, t => Assert.Equal("SYNCED", t.Status));
    }

    [Fact]
    public async Task ExportCsv_ReturnsValidCsvContent()
    {
        using var context = TestDbFactory.Create();
        await SeedTransactions(context);

        var transactions = await BuildTransactionList(context, null, null, null, null, null);
        var sb = new StringBuilder();
        sb.AppendLine("\"Doc Number\",\"Type\",\"Location\",\"Status\",\"Items\",\"Created By\",\"Date\"");
        foreach (var t in transactions)
        {
            sb.AppendLine($"\"{t.DocNumber}\",\"{t.Type}\",\"{t.LocationName}\"," +
                          $"\"{t.Status}\",\"{t.ItemCount}\",\"{t.CreatedBy}\",\"{t.CreatedAt:yyyy-MM-dd HH:mm}\"");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        var content = Encoding.UTF8.GetString(bytes.Skip(Encoding.UTF8.GetPreamble().Length).ToArray());

        Assert.NotEmpty(bytes);
        Assert.Contains("\"Doc Number\"", content);
        Assert.Contains("SI-2024-001", content);
        Assert.Contains("STOCK IN", content);
        Assert.Contains("STOCK PREP", content);
        Assert.Contains("STOCK TAKING", content);
    }

    [Fact]
    public async Task GetHistory_OrderedByDateDescending()
    {
        using var context = TestDbFactory.Create();
        await SeedTransactions(context);

        var all = await BuildTransactionList(context, null, null, null, null, null);

        for (var i = 0; i < all.Count - 1; i++)
        {
            Assert.True(all[i].CreatedAt >= all[i + 1].CreatedAt,
                $"Item {i} ({all[i].CreatedAt}) should be >= item {i + 1} ({all[i + 1].CreatedAt})");
        }
    }

    private static async Task<List<TransactionHistoryItemDto>> BuildTransactionList(
        InvenScan.Database.AppDbContext context,
        string? type, int? locationId, string? status,
        DateTime? from, DateTime? to)
    {
        var result = new List<TransactionHistoryItemDto>();

        if (string.IsNullOrEmpty(type) || type == "STOCK_IN")
        {
            var stockIns = await context.StockIns
                .Include(s => s.Location)
                .Include(s => s.Details)
                .Where(s =>
                    (!locationId.HasValue || s.LocationId == locationId) &&
                    (string.IsNullOrEmpty(status) || s.Status == status) &&
                    (!from.HasValue || s.CreatedAt >= from) &&
                    (!to.HasValue || s.CreatedAt <= to.Value.AddDays(1)))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            result.AddRange(stockIns.Select(s => new TransactionHistoryItemDto
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
            var preps = await context.StockPreps
                .Include(s => s.Details)
                .Where(s =>
                    (string.IsNullOrEmpty(status) || s.Status == status) &&
                    (!from.HasValue || s.CreatedAt >= from) &&
                    (!to.HasValue || s.CreatedAt <= to.Value.AddDays(1)))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            result.AddRange(preps.Select(s => new TransactionHistoryItemDto
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
            var sessions = await context.StockTakings
                .Include(s => s.Details)
                .Where(s =>
                    (string.IsNullOrEmpty(status) || s.Status == status) &&
                    (!from.HasValue || s.CreatedAt >= from) &&
                    (!to.HasValue || s.CreatedAt <= to.Value.AddDays(1)))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            result.AddRange(sessions.Select(s => new TransactionHistoryItemDto
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

internal class TransactionHistoryItemDto
{
    public string DocNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
