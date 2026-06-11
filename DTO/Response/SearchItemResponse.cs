namespace InvenScan.DTO.Response;

public class SearchItemResponse
{
    public int Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public int MinStock { get; set; }
    public string? LocationName { get; set; }
    public string? Status { get; set; }
    public string? EpcTag { get; set; }
}
