using InvenScan.Database;
using InvenScan.DTO.Request;
using InvenScan.DTO.Response;
using InvenScan.Entity;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Service.Implementations;

public class StockPrepService : IStockPrepService
{
    private readonly AppDbContext _context;

    public StockPrepService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<StockPrepResponse>>> GetOpenDocumentsAsync()
    {
        try
        {
            var docs = await _context.StockPreps
                .Include(s => s.Details)
                .Where(s => s.Status == AppConstants.StockPrepStatus.Open || s.Status == AppConstants.StockPrepStatus.InProgress)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return ApiResponse<List<StockPrepResponse>>.Ok(docs.Select(s => MapToResponse(s, false)).ToList());
        }
        catch (Exception)
        {
            return ApiResponse<List<StockPrepResponse>>.Fail("Failed to retrieve stock prep documents.");
        }
    }

    public async Task<ApiResponse<StockPrepResponse>> GetByIdAsync(int id)
    {
        try
        {
            var doc = await _context.StockPreps
                .Include(s => s.Details).ThenInclude(d => d.Item)
                .Include(s => s.Details).ThenInclude(d => d.Location)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (doc == null)
                return ApiResponse<StockPrepResponse>.Fail("Stock prep document not found.");

            return ApiResponse<StockPrepResponse>.Ok(MapToResponse(doc, true));
        }
        catch (Exception)
        {
            return ApiResponse<StockPrepResponse>.Fail("Failed to retrieve stock prep document.");
        }
    }

    public async Task<ApiResponse<StockPrepResponse>> CreateAsync(StockPrepCreateRequest request, string createdBy)
    {
        try
        {
            if (request.Items == null || request.Items.Count == 0)
                return ApiResponse<StockPrepResponse>.Fail("At least one item is required.");

            var docNumber = await GenerateDocNumberAsync();

            var doc = new StockPrep
            {
                DocNumber = docNumber,
                Notes = request.Notes,
                Status = AppConstants.StockPrepStatus.Open,
                CreatedBy = createdBy,
                Details = request.Items.Select(i => new StockPrepDetail
                {
                    ItemId = i.ItemId,
                    LocationId = i.LocationId,
                    RequestedQty = i.RequestedQty,
                    Status = AppConstants.StockPrepDetailStatus.Pending,
                    CreatedBy = createdBy
                }).ToList()
            };

            await _context.StockPreps.AddAsync(doc);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(doc.Id);
        }
        catch (Exception)
        {
            return ApiResponse<StockPrepResponse>.Fail("Failed to create stock prep document.");
        }
    }

    public async Task<ApiResponse<StockPrepResponse>> BulkPickAsync(StockPrepBulkRequest request)
    {
        try
        {
            var doc = await _context.StockPreps
                .Include(s => s.Details)
                .FirstOrDefaultAsync(s => s.Id == request.StockPrepId &&
                    (s.Status == AppConstants.StockPrepStatus.Open || s.Status == AppConstants.StockPrepStatus.InProgress));

            if (doc == null)
                return ApiResponse<StockPrepResponse>.Fail("Open stock prep document not found.");

            foreach (var pickItem in request.Items)
            {
                var detail = doc.Details.FirstOrDefault(d => d.Id == pickItem.DetailId);
                if (detail == null) continue;

                detail.PickedQty = pickItem.PickedQty > 0 ? pickItem.PickedQty : detail.PickedQty + 1;
                detail.ScannedCode = pickItem.ScannedCode;
                detail.UpdatedAt = DateTime.UtcNow;

                if (detail.PickedQty >= detail.RequestedQty)
                    detail.Status = AppConstants.StockPrepDetailStatus.Picked;
            }

            var allPicked = doc.Details.All(d => d.Status == AppConstants.StockPrepDetailStatus.Picked);
            doc.Status = allPicked ? AppConstants.StockPrepStatus.Done : AppConstants.StockPrepStatus.InProgress;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(doc.Id);
        }
        catch (Exception)
        {
            return ApiResponse<StockPrepResponse>.Fail("Failed to process picked items.");
        }
    }

    private async Task<string> GenerateDocNumberAsync()
    {
        var prefix = $"SP-{DateTime.UtcNow:yyyyMMdd}-";
        var count = await _context.StockPreps.CountAsync(s => s.DocNumber.StartsWith(prefix));
        return $"{prefix}{(count + 1):D3}";
    }

    private static StockPrepResponse MapToResponse(StockPrep doc, bool includeDetails) => new()
    {
        Id = doc.Id,
        DocNumber = doc.DocNumber,
        Notes = doc.Notes,
        Status = doc.Status,
        CreatedBy = doc.CreatedBy,
        CreatedAt = doc.CreatedAt,
        TotalItems = doc.Details.Count,
        PickedItems = doc.Details.Count(d => d.Status == AppConstants.StockPrepDetailStatus.Picked),
        Details = includeDetails ? doc.Details.Select(d => new StockPrepDetailResponse
        {
            Id = d.Id,
            ItemId = d.ItemId,
            ItemCode = d.Item?.ItemCode ?? string.Empty,
            ItemName = d.Item?.ItemName ?? string.Empty,
            Unit = d.Item?.Unit ?? string.Empty,
            LocationId = d.LocationId,
            LocationCode = d.Location?.LocationCode ?? string.Empty,
            LocationName = d.Location?.LocationName ?? string.Empty,
            RequestedQty = d.RequestedQty,
            PickedQty = d.PickedQty,
            Status = d.Status,
            ScannedCode = d.ScannedCode
        }).ToList() : new()
    };
}
