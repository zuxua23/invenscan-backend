namespace InvenScan.DTO.Request;

public class StockTakingCreateRequest
{
    public int LocationId { get; set; }
    public string Remark { get; set; } = string.Empty;
}
