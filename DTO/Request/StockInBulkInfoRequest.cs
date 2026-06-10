namespace InvenScan.DTO.Request;

public class StockInBulkInfoRequest
{
    public List<string> Codes { get; set; } = new();
    public string ScannerType { get; set; } = "RFID";
}
