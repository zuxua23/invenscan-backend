namespace InvenScan.DTO.Request;

public class StockInSubmitRequest
{
    public int LocationId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<StockInScanItem> Items { get; set; } = new();
}

public class StockInScanItem
{
    public int? TagId { get; set; }
    public int ItemId { get; set; }
    public string ScannedCode { get; set; } = string.Empty;
    public string ScanType { get; set; } = "RFID";
}
