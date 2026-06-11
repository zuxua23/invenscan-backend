using InvenScan.Database;
using InvenScan.DTO.Request;
using InvenScan.DTO.Response;
using InvenScan.Entity;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Service.Implementations;

public class StockTakingService : IStockTakingService
{
    private readonly AppDbContext _context;

    public StockTakingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<StockTakingResponse>> CreateSessionAsync(StockTakingCreateRequest request, string createdBy)
    {
        try
        {
            var hasOpen = await _context.StockTakings.AnyAsync(s => s.Status == AppConstants.StockTakingStatus.Open);
            if (hasOpen)
                return ApiResponse<StockTakingResponse>.Fail("There is already an active stock taking session. Close it before creating a new one.");

            var sessionCode = await GenerateSessionCodeAsync();

            var session = new StockTaking
            {
                SessionCode = sessionCode,
                Remark = request.Remark,
                Status = AppConstants.StockTakingStatus.Open,
                CreatedBy = createdBy
            };

            await _context.StockTakings.AddAsync(session);
            await _context.SaveChangesAsync();

            var inStockTags = await _context.Tags
                .Where(t => t.Status == AppConstants.TagStatus.InStock)
                .ToListAsync();

            var details = inStockTags.Select(t => new StockTakingDetail
            {
                SttId = session.Id,
                TagId = t.Id,
                ItemId = t.ItemId,
                Action = AppConstants.StockTakingAction.System
            }).ToList();

            await _context.StockTakingDetails.AddRangeAsync(details);
            await _context.SaveChangesAsync();

            return ApiResponse<StockTakingResponse>.Ok(MapToResponse(session, details.Count, 0, 0), "Session created successfully.");
        }
        catch (Exception)
        {
            return ApiResponse<StockTakingResponse>.Fail("Failed to create stock taking session.");
        }
    }

    public async Task<ApiResponse<List<StockTakingResponse>>> GetAllSessionsAsync()
    {
        try
        {
            var sessions = await _context.StockTakings
                .Include(s => s.Details)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var responses = sessions.Select(s => MapToResponse(
                s,
                s.Details.Count,
                s.Details.Count(d => d.Action == AppConstants.StockTakingAction.Scan),
                s.Details.Count(d => d.Action == AppConstants.StockTakingAction.Missing)
            )).ToList();

            return ApiResponse<List<StockTakingResponse>>.Ok(responses);
        }
        catch (Exception)
        {
            return ApiResponse<List<StockTakingResponse>>.Fail("Failed to retrieve sessions.");
        }
    }

    public async Task<ApiResponse<StockTakingResponse>> GetActiveSessionAsync()
    {
        try
        {
            var session = await _context.StockTakings
                .Include(s => s.Details)
                .FirstOrDefaultAsync(s => s.Status == AppConstants.StockTakingStatus.Open);

            if (session == null)
                return ApiResponse<StockTakingResponse>.Fail("No active stock taking session.");

            return ApiResponse<StockTakingResponse>.Ok(MapToResponse(
                session,
                session.Details.Count,
                session.Details.Count(d => d.Action == AppConstants.StockTakingAction.Scan),
                session.Details.Count(d => d.Action == AppConstants.StockTakingAction.Missing)
            ));
        }
        catch (Exception)
        {
            return ApiResponse<StockTakingResponse>.Fail("Failed to retrieve active session.");
        }
    }

    public async Task<ApiResponse<List<StockTakingDetailResponse>>> GetSessionTagsAsync(int sttId)
    {
        try
        {
            var exists = await _context.StockTakings.AnyAsync(s => s.Id == sttId);
            if (!exists)
                return ApiResponse<List<StockTakingDetailResponse>>.Fail("Session not found.");

            var details = await _context.StockTakingDetails
                .Include(d => d.Tag).ThenInclude(t => t.Location)
                .Include(d => d.Item)
                .Where(d => d.SttId == sttId)
                .OrderBy(d => d.Item.ItemCode)
                .Select(d => new StockTakingDetailResponse
                {
                    Id = d.Id,
                    SttId = d.SttId,
                    TagId = d.TagId,
                    EpcTag = d.Tag.EpcTag,
                    ItemId = d.ItemId,
                    ItemCode = d.Item.ItemCode,
                    ItemName = d.Item.ItemName,
                    LocationCode = d.Tag.Location.LocationCode,
                    LocationName = d.Tag.Location.LocationName,
                    Action = d.Action,
                    ScannedAt = d.ScannedAt
                })
                .ToListAsync();

            return ApiResponse<List<StockTakingDetailResponse>>.Ok(details);
        }
        catch (Exception)
        {
            return ApiResponse<List<StockTakingDetailResponse>>.Fail("Failed to retrieve session tags.");
        }
    }

    public async Task<ApiResponse<List<TagResponse>>> GetAvailableTagsAsync(int sttId)
    {
        try
        {
            var sessionTagIds = await _context.StockTakingDetails
                .Where(d => d.SttId == sttId)
                .Select(d => d.TagId)
                .ToListAsync();

            var tags = await _context.Tags
                .Include(t => t.Item)
                .Include(t => t.Location)
                .Where(t => t.Status == AppConstants.TagStatus.InStock && !sessionTagIds.Contains(t.Id))
                .ToListAsync();

            var responses = tags.Select(t => new TagResponse
            {
                Id = t.Id,
                TagId = t.TagId,
                EpcTag = t.EpcTag,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                Item = new ItemResponse { Id = t.Item.Id, ItemCode = t.Item.ItemCode, ItemName = t.Item.ItemName },
                Location = new LocationResponse { Id = t.Location.Id, LocationCode = t.Location.LocationCode, LocationName = t.Location.LocationName }
            }).ToList();

            return ApiResponse<List<TagResponse>>.Ok(responses);
        }
        catch (Exception)
        {
            return ApiResponse<List<TagResponse>>.Fail("Failed to retrieve available tags.");
        }
    }

    public async Task<ApiResponse<StockTakingResponse>> OperatorSubmitAsync(StockTakingOperatorSubmitRequest request, string operatorId)
    {
        try
        {
            var session = await _context.StockTakings
                .Include(s => s.Details)
                .FirstOrDefaultAsync(s => s.Id == request.SttId && s.Status == AppConstants.StockTakingStatus.Open);

            if (session == null)
                return ApiResponse<StockTakingResponse>.Fail("Active session not found.");

            var scannedTagIds = new HashSet<int>();
            foreach (var scan in request.ScannedTags)
            {
                if (scan.Action == AppConstants.StockTakingAction.Scan && int.TryParse(scan.TagId, out var tagId))
                    scannedTagIds.Add(tagId);
            }

            foreach (var detail in session.Details)
            {
                var isScanned = scannedTagIds.Contains(detail.TagId);
                detail.Action = isScanned ? AppConstants.StockTakingAction.Scan : AppConstants.StockTakingAction.Missing;
                detail.CreatedBy = operatorId;
                if (isScanned)
                    detail.ScannedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return ApiResponse<StockTakingResponse>.Ok(MapToResponse(
                session,
                session.Details.Count,
                session.Details.Count(d => d.Action == AppConstants.StockTakingAction.Scan),
                session.Details.Count(d => d.Action == AppConstants.StockTakingAction.Missing)
            ), "Scan results submitted successfully.");
        }
        catch (Exception)
        {
            return ApiResponse<StockTakingResponse>.Fail("Failed to submit scan results.");
        }
    }

    public async Task<ApiResponse<StockTakingResponse>> CloseSessionAsync(int sttId, string closedBy)
    {
        try
        {
            var session = await _context.StockTakings
                .Include(s => s.Details)
                .FirstOrDefaultAsync(s => s.Id == sttId && s.Status == AppConstants.StockTakingStatus.Open);

            if (session == null)
                return ApiResponse<StockTakingResponse>.Fail("Active session not found.");

            session.Status = AppConstants.StockTakingStatus.Closed;
            session.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<StockTakingResponse>.Ok(MapToResponse(
                session,
                session.Details.Count,
                session.Details.Count(d => d.Action == AppConstants.StockTakingAction.Scan),
                session.Details.Count(d => d.Action == AppConstants.StockTakingAction.Missing)
            ), "Session closed successfully.");
        }
        catch (Exception)
        {
            return ApiResponse<StockTakingResponse>.Fail("Failed to close session.");
        }
    }

    private async Task<string> GenerateSessionCodeAsync()
    {
        var prefix = $"STT-{DateTime.UtcNow:yyyyMMdd}-";
        var count = await _context.StockTakings.CountAsync(s => s.SessionCode.StartsWith(prefix));
        return $"{prefix}{(count + 1):D3}";
    }

    private static StockTakingResponse MapToResponse(StockTaking s, int total, int scanned, int missing) => new()
    {
        Id = s.Id,
        SessionCode = s.SessionCode,
        Remark = s.Remark,
        Status = s.Status,
        CreatedBy = s.CreatedBy,
        CreatedAt = s.CreatedAt,
        ClosedAt = s.ClosedAt,
        TotalItems = total,
        ScannedItems = scanned,
        MissingItems = missing
    };
}
