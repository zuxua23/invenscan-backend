using InvenScan.DTO.Response;

namespace InvenScan.Service.Interfaces;

public interface IItemService
{
    Task<ApiResponse<List<ItemResponse>>> GetAllItemsAsync();
    Task<ApiResponse<ItemResponse>> GetItemByIdAsync(int id);
}
