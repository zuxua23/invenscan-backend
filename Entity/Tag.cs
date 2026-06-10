namespace InvenScan.Entity;

public class Tag
{
    public int Id { get; set; }
    public string TagId { get; set; } = string.Empty;
    public string EpcTag { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public int LocationId { get; set; }
    public string Status { get; set; } = "IN_STOCK";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Item Item { get; set; } = null!;
    public Location Location { get; set; } = null!;
}
