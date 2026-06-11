using InvenScan.Database;
using InvenScan.DTO.Response;
using InvenScan.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Service.Implementations;

public class SearchItemService : ISearchItemService
{
    private readonly AppDbContext _context;

    public SearchItemService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<SearchItemResponse>>> GetAllItemsWithTagsAsync()
    {
        try
        {
            var items = await _context.Items
                .Include(i => i.Tags).ThenInclude(t => t.Location)
                .Where(i => !i.IsDelete)
                .OrderBy(i => i.ItemCode)
                .ToListAsync();

            return ApiResponse<List<SearchItemResponse>>.Ok(items.Select(MapToResponse).ToList());
        }
        catch (Exception)
        {
            return ApiResponse<List<SearchItemResponse>>.Fail("Failed to retrieve items.");
        }
    }

    public async Task<ApiResponse<SearchItemResponse>> GetByCodeAsync(string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
                return ApiResponse<SearchItemResponse>.Fail("Code is required.");

            var item = await _context.Items
                .Include(i => i.Tags).ThenInclude(t => t.Location)
                .FirstOrDefaultAsync(i => !i.IsDelete && (i.ItemCode == code || i.Tags.Any(t => t.EpcTag == code)));

            if (item == null)
                return ApiResponse<SearchItemResponse>.Fail("Item not found for code: " + code);

            return ApiResponse<SearchItemResponse>.Ok(MapToResponse(item));
        }
        catch (Exception)
        {
            return ApiResponse<SearchItemResponse>.Fail("Search failed.");
        }
    }

    private static SearchItemResponse MapToResponse(Entity.Item item)
    {
        var representativeTag = item.Tags.FirstOrDefault();
        return new SearchItemResponse
        {
            Id = item.Id,
            ItemCode = item.ItemCode,
            ItemName = item.ItemName,
            Description = item.Description,
            Unit = item.Unit,
            MinStock = item.MinStock,
            LocationName = representativeTag?.Location?.LocationName,
            Status = representativeTag?.Status,
            EpcTag = representativeTag?.EpcTag
        };
    }
}
