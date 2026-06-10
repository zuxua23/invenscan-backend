namespace InvenScan.DTO.Response;

public class ItemResponse
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int MinStock { get; set; }
    public DateTime CreatedAt { get; set; }
}
