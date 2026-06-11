namespace InvenScan.DTO.Request;

public class StockPrepBulkRequest
{
    public int StockPrepId { get; set; }
    public List<StockPrepPickItem> Items { get; set; } = new();
}

public class StockPrepPickItem
{
    public int DetailId { get; set; }
    public string ScannedCode { get; set; } = string.Empty;
    public string ScanType { get; set; } = "RFID";
    public int PickedQty { get; set; }
}
