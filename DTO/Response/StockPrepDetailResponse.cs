namespace InvenScan.DTO.Response;

public class StockPrepDetailResponse
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public int RequestedQty { get; set; }
    public int PickedQty { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ScannedCode { get; set; } = string.Empty;
}
