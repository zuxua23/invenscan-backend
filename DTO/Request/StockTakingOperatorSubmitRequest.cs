namespace InvenScan.DTO.Request;

public class StockTakingOperatorSubmitRequest
{
    public int SttId { get; set; }
    public List<string> ScannedEpcs { get; set; } = new();
    public string OperatorId { get; set; } = string.Empty;
}
