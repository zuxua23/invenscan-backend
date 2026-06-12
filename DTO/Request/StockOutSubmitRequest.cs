namespace InvenScan.DTO.Request;

public class StockOutSubmitRequest
{
    public int LocationId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<StockOutScanItem> Details { get; set; } = new();
}

public class StockOutScanItem
{
    public int? TagId { get; set; }
    public int ItemId { get; set; }
    public string ScannedCode { get; set; } = string.Empty;
    public string ScanType { get; set; } = "RFID";
}
