using InvenScan.DTO.Request;
using InvenScan.DTO.Response;

namespace InvenScan.Service.Interfaces;

public interface IStockOutService
{
    Task<ApiResponse<StockOutLookupResponse>> LookupScanCodeAsync(string code, string scannerType);
    Task<ApiResponse<StockOutResponse>> SubmitStockOutAsync(StockOutSubmitRequest request, string createdBy);
    Task<ApiResponse<List<StockOutLookupResponse>>> BulkInfoAsync(string[] codes, string scannerType);
}
