using InvenScan.Database;
using InvenScan.DTO.Response;
using InvenScan.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvenScan.Service.Implementations;

public class ItemService : IItemService
{
    private readonly AppDbContext _context;

    public ItemService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<ItemResponse>>> GetAllItemsAsync()
    {
        try
        {
            var items = await _context.Items
                .Where(i => !i.IsDelete)
                .OrderBy(i => i.ItemCode)
                .Select(i => new ItemResponse
                {
                    Id = i.Id,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    Description = i.Description,
                    Unit = i.Unit,
                    MinStock = i.MinStock,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<ItemResponse>>.Ok(items);
        }
        catch (Exception)
        {
            return ApiResponse<List<ItemResponse>>.Fail("Failed to retrieve items.");
        }
    }

    public async Task<ApiResponse<ItemResponse>> GetItemByIdAsync(int id)
    {
        try
        {
            var item = await _context.Items
                .Where(i => i.Id == id && !i.IsDelete)
                .Select(i => new ItemResponse
                {
                    Id = i.Id,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    Description = i.Description,
                    Unit = i.Unit,
                    MinStock = i.MinStock,
                    CreatedAt = i.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (item == null)
                return ApiResponse<ItemResponse>.Fail("Item not found.");

            return ApiResponse<ItemResponse>.Ok(item);
        }
        catch (Exception)
        {
            return ApiResponse<ItemResponse>.Fail("Failed to retrieve item.");
        }
    }
}
