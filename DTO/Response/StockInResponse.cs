namespace InvenScan.DTO.Response;

public class StockInResponse
{
    public int Id { get; set; }
    public string DocNumber { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalItems { get; set; }
}
