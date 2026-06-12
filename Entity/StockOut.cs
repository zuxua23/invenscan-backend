namespace InvenScan.Entity;

public class StockOut
{
    public int Id { get; set; }
    public string DocNumber { get; set; } = string.Empty;
    public int LocationId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "PENDING";

    public Location Location { get; set; } = null!;
    public ICollection<StockOutDetail> Details { get; set; } = new List<StockOutDetail>();
}
