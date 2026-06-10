using InvenScan.DTO.Response;

namespace InvenScan.Service.Interfaces;

public interface ISearchItemService
{
    Task<ApiResponse<List<SearchItemResponse>>> GetAllItemsWithTagsAsync();
    Task<ApiResponse<SearchItemResponse>> GetByCodeAsync(string code);
}
