namespace InvenScan.DTO.Response;

public class StockTakingResponse
{
    public int Id { get; set; }
    public string SessionCode { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int TotalItems { get; set; }
    public int ScannedItems { get; set; }
    public int MissingItems { get; set; }
}
