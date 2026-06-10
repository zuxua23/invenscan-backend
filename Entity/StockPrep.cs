namespace InvenScan.Entity;

public class StockPrep
{
    public int Id { get; set; }
    public string DocNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = "OPEN";
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<StockPrepDetail> Details { get; set; } = new List<StockPrepDetail>();
}
