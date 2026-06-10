namespace InvenScan.Entity;

public class Location
{
    public int Id { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDelete { get; set; } = false;

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<StockIn> StockIns { get; set; } = new List<StockIn>();
}
