namespace InvenScan.Entity;

public class StockTaking
{
    public int Id { get; set; }
    public string SessionCode { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public string Status { get; set; } = "OPEN";
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public ICollection<StockTakingDetail> Details { get; set; } = new List<StockTakingDetail>();
}
