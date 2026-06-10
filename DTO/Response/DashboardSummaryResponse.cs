namespace InvenScan.DTO.Response;

public class DashboardSummaryResponse
{
    public int TotalItems { get; set; }
    public int TotalLocations { get; set; }
    public int TotalTags { get; set; }
    public int ActiveStockTakingSessions { get; set; }
    public int PendingStockPrepDocs { get; set; }
    public int TotalUsers { get; set; }
}
