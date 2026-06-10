namespace InvenScan.DTO.Response;

public class StockPrepResponse
{
    public int Id { get; set; }
    public string DocNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalItems { get; set; }
    public int PickedItems { get; set; }
    public List<StockPrepDetailResponse> Details { get; set; } = new();
}
