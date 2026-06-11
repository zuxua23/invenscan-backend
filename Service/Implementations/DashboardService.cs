using InvenScan.Database;
using InvenScan.DTO.Response;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Service.Implementations;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync()
    {
        var totalItems = await _context.Items.CountAsync(i => !i.IsDelete);
        var totalLocations = await _context.Locations.CountAsync(l => !l.IsDelete);
        var totalTags = await _context.Tags.CountAsync();
        var activeSessions = await _context.StockTakings.CountAsync(s => s.Status == AppConstants.StockTakingStatus.Open);
        var pendingPrep = await _context.StockPreps.CountAsync(s =>
            s.Status == AppConstants.StockPrepStatus.Open || s.Status == AppConstants.StockPrepStatus.InProgress);
        var totalUsers = await _context.Users.CountAsync(u => u.IsActive);

        return new DashboardSummaryResponse
        {
            TotalItems = totalItems,
            TotalLocations = totalLocations,
            TotalTags = totalTags,
            ActiveStockTakingSessions = activeSessions,
            PendingStockPrepDocs = pendingPrep,
            TotalUsers = totalUsers
        };
    }

    public async Task<DashboardChartDataResponse> GetChartDataAsync()
    {
        var today = DateTime.UtcNow.Date;

        var stockInByDay = await _context.StockIns
            .Where(s => s.CreatedAt >= today.AddDays(-6))
            .GroupBy(s => s.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var stockInLabels = new List<string>();
        var stockInValues = new List<int>();
        for (var i = 6; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            stockInLabels.Add(day.ToString("MMM dd"));
            stockInValues.Add(stockInByDay.FirstOrDefault(x => x.Date == day)?.Count ?? 0);
        }

        var tagStatusGroups = await _context.Tags
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var tagStatuses = new[] { "IN_STOCK", "OUT", "UNKNOWN" };
        var tagLabels = new List<string> { "In Stock", "Out", "Unknown" };
        var tagValues = tagStatuses.Select(s =>
            tagStatusGroups.FirstOrDefault(g => g.Status == s)?.Count ?? 0).ToList();

        var fourWeeksAgo = today.AddDays(-27);
        var sttByWeek = await _context.StockTakings
            .Where(s => s.CreatedAt >= fourWeeksAgo)
            .ToListAsync();

        var sttLabels = new List<string>();
        var sttValues = new List<int>();
        for (var i = 3; i >= 0; i--)
        {
            var weekStart = today.AddDays(-i * 7 - 6);
            var weekEnd = today.AddDays(-i * 7);
            sttLabels.Add($"W{4 - i} ({weekStart:MMM dd})");
            sttValues.Add(sttByWeek.Count(s => s.CreatedAt.Date >= weekStart && s.CreatedAt.Date <= weekEnd));
        }

        return new DashboardChartDataResponse
        {
            StockInLabels = stockInLabels,
            StockInValues = stockInValues,
            TagStatusLabels = tagLabels,
            TagStatusValues = tagValues,
            StockTakingLabels = sttLabels,
            StockTakingValues = sttValues
        };
    }
}
