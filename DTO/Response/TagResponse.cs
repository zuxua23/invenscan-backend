namespace InvenScan.DTO.Response;

public class TagResponse
{
    public int Id { get; set; }
    public string TagId { get; set; } = string.Empty;
    public string EpcTag { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ItemResponse? Item { get; set; }
    public LocationResponse? Location { get; set; }
}
