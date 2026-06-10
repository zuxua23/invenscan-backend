namespace InvenScan.DTO.Response;

public class StockTakingDetailResponse
{
    public int Id { get; set; }
    public int SttId { get; set; }
    public int TagId { get; set; }
    public string EpcTag { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime? ScannedAt { get; set; }
}
