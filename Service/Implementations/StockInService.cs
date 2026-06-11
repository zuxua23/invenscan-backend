using InvenScan.Database;
using InvenScan.DTO.Request;
using InvenScan.DTO.Response;
using InvenScan.Entity;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Service.Implementations;

public class StockInService : IStockInService
{
    private readonly AppDbContext _context;

    public StockInService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<StockInLookupResponse>> LookupScanCodeAsync(string code, string scannerType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
                return ApiResponse<StockInLookupResponse>.Fail("Scan code is required.");

            if (scannerType == AppConstants.ScanType.Rfid)
            {
                var tag = await _context.Tags
                    .Include(t => t.Item)
                    .Include(t => t.Location)
                    .FirstOrDefaultAsync(t => t.EpcTag == code);

                if (tag == null)
                    return ApiResponse<StockInLookupResponse>.Fail("Tag not found for EPC: " + code);

                return ApiResponse<StockInLookupResponse>.Ok(new StockInLookupResponse
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
                    return ApiResponse<StockInLookupResponse>.Fail("Item not found for code: " + code);

                return ApiResponse<StockInLookupResponse>.Ok(new StockInLookupResponse
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
            return ApiResponse<StockInLookupResponse>.Fail("Lookup failed.");
        }
    }

    public async Task<ApiResponse<StockInResponse>> SubmitStockInAsync(StockInSubmitRequest request, string createdBy)
    {
        try
        {
            if (request.Details == null || request.Details.Count == 0)
                return ApiResponse<StockInResponse>.Fail("At least one item is required.");

            var locationExists = await _context.Locations.AnyAsync(l => l.Id == request.LocationId && !l.IsDelete);
            if (!locationExists)
                return ApiResponse<StockInResponse>.Fail("Location not found.");

            var docNumber = await GenerateDocNumberAsync();

            var stockIn = new StockIn
            {
                DocNumber = docNumber,
                LocationId = request.LocationId,
                Notes = request.Notes,
                CreatedBy = createdBy,
                Status = AppConstants.StockInStatus.Synced,
                Details = request.Details.Select(i => new StockInDetail
                {
                    TagId = i.TagId,
                    ItemId = i.ItemId,
                    ScannedCode = i.ScannedCode,
                    ScanType = i.ScanType
                }).ToList()
            };

            await _context.StockIns.AddAsync(stockIn);
            await _context.SaveChangesAsync();

            var location = await _context.Locations.FindAsync(request.LocationId);

            return ApiResponse<StockInResponse>.Ok(new StockInResponse
            {
                Id = stockIn.Id,
                DocNumber = stockIn.DocNumber,
                LocationName = location?.LocationName ?? string.Empty,
                Notes = stockIn.Notes,
                Status = stockIn.Status,
                CreatedBy = stockIn.CreatedBy,
                CreatedAt = stockIn.CreatedAt,
                TotalItems = stockIn.Details.Count
            }, "Stock in submitted successfully.");
        }
        catch (Exception)
        {
            return ApiResponse<StockInResponse>.Fail("Failed to submit stock in.");
        }
    }

    public async Task<ApiResponse<List<StockInLookupResponse>>> BulkInfoAsync(StockInBulkInfoRequest request)
    {
        try
        {
            if (request.Codes == null || request.Codes.Count == 0)
                return ApiResponse<List<StockInLookupResponse>>.Fail("No codes provided.");

            var results = new List<StockInLookupResponse>();

            foreach (var code in request.Codes)
            {
                var lookupResult = await LookupScanCodeAsync(code, request.ScannerType);
                if (lookupResult.Success && lookupResult.Data != null)
                    results.Add(lookupResult.Data);
            }

            return ApiResponse<List<StockInLookupResponse>>.Ok(results);
        }
        catch (Exception)
        {
            return ApiResponse<List<StockInLookupResponse>>.Fail("Bulk info lookup failed.");
        }
    }

    private async Task<string> GenerateDocNumberAsync()
    {
        var prefix = $"SI-{DateTime.UtcNow:yyyyMMdd}-";
        var count = await _context.StockIns.CountAsync(s => s.DocNumber.StartsWith(prefix));
        return $"{prefix}{(count + 1):D3}";
    }
}
