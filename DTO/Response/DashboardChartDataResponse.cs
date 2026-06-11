namespace InvenScan.DTO.Response;

public class DashboardChartDataResponse
{
    public List<string> StockInLabels { get; set; } = new();
    public List<int> StockInValues { get; set; } = new();

    public List<string> TagStatusLabels { get; set; } = new();
    public List<int> TagStatusValues { get; set; } = new();

    public List<string> StockTakingLabels { get; set; } = new();
    public List<int> StockTakingValues { get; set; } = new();
}
