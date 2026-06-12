using InvenScan.Database;
using InvenScan.DTO.Request;
using InvenScan.DTO.Response;
using InvenScan.Entity;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Service.Implementations;

public class StockOutService : IStockOutService
{
    private readonly AppDbContext _context;

    public StockOutService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<StockOutLookupResponse>> LookupScanCodeAsync(string code, string scannerType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
                return ApiResponse<StockOutLookupResponse>.Fail("Scan code is required.");

            if (scannerType == AppConstants.ScanType.Rfid)
            {
                var tag = await _context.Tags
                    .Include(t => t.Item)
                    .Include(t => t.Location)
                    .FirstOrDefaultAsync(t => t.EpcTag == code);

                if (tag == null)
                    return ApiResponse<StockOutLookupResponse>.Fail("Tag not found for EPC: " + code);

                return ApiResponse<StockOutLookupResponse>.Ok(new StockOutLookupResponse
                {
                    ScannedCode = code,
                    ScanType = AppConstants.ScanType.Rfid,
                    TagId = tag.Id,
                    EpcTag = tag.EpcTag,
                    ItemId = tag.Item.Id,
                    ItemCode = tag.Item.ItemCode,
                    ItemName = tag.Item.ItemName,
                    Unit = tag.Item.Unit,
                    LocationCode = tag.Location.LocationCode,
                    LocationName = tag.Location.LocationName,
                    TagStatus = tag.Status
                });
            }
            else
            {
                var item = await _context.Items
                    .FirstOrDefaultAsync(i => i.ItemCode == code && !i.IsDelete);

                if (item == null)
                    return ApiResponse<StockOutLookupResponse>.Fail("Item not found for code: " + code);

                return ApiResponse<StockOutLookupResponse>.Ok(new StockOutLookupResponse
                {
                    ScannedCode = code,
                    ScanType = AppConstants.ScanType.Barcode,
                    ItemId = item.Id,
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    Unit = item.Unit
                });
            }
        }
        catch (Exception)
        {
            return ApiResponse<StockOutLookupResponse>.Fail("Lookup failed.");
        }
    }

    public async Task<ApiResponse<StockOutResponse>> SubmitStockOutAsync(StockOutSubmitRequest request, string createdBy)
    {
        try
        {
            if (request.Details == null || request.Details.Count == 0)
                return ApiResponse<StockOutResponse>.Fail("At least one item is required.");

            var locationExists = await _context.Locations.AnyAsync(l => l.Id == request.LocationId && !l.IsDelete);
            if (!locationExists)
                return ApiResponse<StockOutResponse>.Fail("Location not found.");

            var docNumber = await GenerateDocNumberAsync();

            var stockOut = new StockOut
            {
                DocNumber = docNumber,
                LocationId = request.LocationId,
                Notes = request.Notes,
                CreatedBy = createdBy,
                Status = AppConstants.StockOutStatus.Synced,
                Details = request.Details.Select(i => new StockOutDetail
                {
                    TagId = i.TagId,
                    ItemId = i.ItemId,
                    ScannedCode = i.ScannedCode,
                    ScanType = i.ScanType
                }).ToList()
            };

            await _context.StockOuts.AddAsync(stockOut);

            var tagIds = request.Details
                .Where(d => d.TagId.HasValue)
                .Select(d => d.TagId!.Value)
                .Distinct()
                .ToList();

            if (tagIds.Count > 0)
            {
                var tags = await _context.Tags
                    .Where(t => tagIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var tag in tags)
                {
                    tag.Status = AppConstants.TagStatus.Out;
                    tag.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            var location = await _context.Locations.FindAsync(request.LocationId);

            return ApiResponse<StockOutResponse>.Ok(new StockOutResponse
            {
                Id = stockOut.Id,
                DocNumber = stockOut.DocNumber,
                LocationName = location?.LocationName ?? string.Empty,
                Notes = stockOut.Notes,
                Status = stockOut.Status,
                CreatedBy = stockOut.CreatedBy,
                CreatedAt = stockOut.CreatedAt,
                TotalItems = stockOut.Details.Count
            }, "Stock out submitted successfully.");
        }
        catch (Exception)
        {
            return ApiResponse<StockOutResponse>.Fail("Failed to submit stock out.");
        }
    }

    public async Task<ApiResponse<List<StockOutLookupResponse>>> BulkInfoAsync(string[] codes, string scannerType)
    {
        try
        {
            if (codes == null || codes.Length == 0)
                return ApiResponse<List<StockOutLookupResponse>>.Fail("No codes provided.");

            var results = new List<StockOutLookupResponse>();
            foreach (var code in codes)
            {
                var result = await LookupScanCodeAsync(code, scannerType);
                if (result.Success && result.Data != null)
                    results.Add(result.Data);
            }

            return ApiResponse<List<StockOutLookupResponse>>.Ok(results);
        }
        catch (Exception)
        {
            return ApiResponse<List<StockOutLookupResponse>>.Fail("Bulk info lookup failed.");
        }
    }

    private async Task<string> GenerateDocNumberAsync()
    {
        var prefix = $"SO-{DateTime.UtcNow:yyyyMMdd}-";
        var count = await _context.StockOuts.CountAsync(s => s.DocNumber.StartsWith(prefix));
        return $"{prefix}{(count + 1):D3}";
    }
}
