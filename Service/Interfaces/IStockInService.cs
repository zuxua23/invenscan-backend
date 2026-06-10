using InvenScan.DTO.Request;
using InvenScan.DTO.Response;

namespace InvenScan.Service.Interfaces;

public interface IStockInService
{
    Task<ApiResponse<StockInLookupResponse>> LookupScanCodeAsync(string code, string scannerType);
    Task<ApiResponse<StockInResponse>> SubmitStockInAsync(StockInSubmitRequest request, string createdBy);
    Task<ApiResponse<List<StockInLookupResponse>>> BulkInfoAsync(StockInBulkInfoRequest request);
}
