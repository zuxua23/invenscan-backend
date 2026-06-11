using InvenScan.DTO.Response;

namespace InvenScan.Service.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync();
    Task<DashboardChartDataResponse> GetChartDataAsync();
}
