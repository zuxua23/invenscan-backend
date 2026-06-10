using InvenScan.DTO.Request;
using InvenScan.DTO.Response;

namespace InvenScan.Service.Interfaces;

public interface IStockTakingService
{
    Task<ApiResponse<StockTakingResponse>> CreateSessionAsync(StockTakingCreateRequest request, string createdBy);
    Task<ApiResponse<List<StockTakingResponse>>> GetAllSessionsAsync();
    Task<ApiResponse<StockTakingResponse>> GetActiveSessionAsync();
    Task<ApiResponse<List<StockTakingDetailResponse>>> GetSessionTagsAsync(int sttId);
    Task<ApiResponse<List<TagResponse>>> GetAvailableTagsAsync(int sttId);
    Task<ApiResponse<StockTakingResponse>> OperatorSubmitAsync(StockTakingOperatorSubmitRequest request);
    Task<ApiResponse<StockTakingResponse>> CloseSessionAsync(int sttId, string closedBy);
}
