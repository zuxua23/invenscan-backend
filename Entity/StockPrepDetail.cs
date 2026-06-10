namespace InvenScan.Entity;

public class StockPrepDetail
{
    public int Id { get; set; }
    public int StockPrepId { get; set; }
    public int ItemId { get; set; }
    public int LocationId { get; set; }
    public int RequestedQty { get; set; }
    public int PickedQty { get; set; } = 0;
    public string Status { get; set; } = "PENDING";
    public string ScannedCode { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }

    public StockPrep StockPrep { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public Location Location { get; set; } = null!;
}
