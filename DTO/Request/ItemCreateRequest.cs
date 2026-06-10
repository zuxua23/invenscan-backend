namespace InvenScan.DTO.Request;

public class ItemCreateRequest
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int MinStock { get; set; } = 0;
}
