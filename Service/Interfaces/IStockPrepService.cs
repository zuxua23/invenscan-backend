using InvenScan.DTO.Request;
using InvenScan.DTO.Response;

namespace InvenScan.Service.Interfaces;

public interface IStockPrepService
{
    Task<ApiResponse<List<StockPrepResponse>>> GetOpenDocumentsAsync();
    Task<ApiResponse<StockPrepResponse>> GetByIdAsync(int id);
    Task<ApiResponse<StockPrepResponse>> CreateAsync(StockPrepCreateRequest request, string createdBy);
    Task<ApiResponse<StockPrepResponse>> BulkPickAsync(StockPrepBulkRequest request);
}
