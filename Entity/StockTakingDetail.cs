namespace InvenScan.Entity;

public class StockTakingDetail
{
    public int Id { get; set; }
    public int SttId { get; set; }
    public int TagId { get; set; }
    public int ItemId { get; set; }
    public string Action { get; set; } = "SYSTEM";
    public DateTime? ScannedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public StockTaking StockTaking { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
