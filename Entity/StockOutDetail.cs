namespace InvenScan.Entity;

public class StockOutDetail
{
    public int Id { get; set; }
    public int StockOutId { get; set; }
    public int? TagId { get; set; }
    public int ItemId { get; set; }
    public string ScannedCode { get; set; } = string.Empty;
    public string ScanType { get; set; } = "RFID";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public StockOut StockOut { get; set; } = null!;
    public Tag? Tag { get; set; }
    public Item Item { get; set; } = null!;
}
