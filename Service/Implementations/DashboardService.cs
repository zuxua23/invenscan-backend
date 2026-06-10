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
}
