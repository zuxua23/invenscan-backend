namespace InvenScan.DTO.Request;

public class StockTakingOperatorSubmitRequest
{
    public int SttId { get; set; }
    public List<StockTakingScanItem> ScannedTags { get; set; } = new();
}

public class StockTakingScanItem
{
    public string TagId { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string Action { get; set; } = string.Empty;
    public long ScannedAt { get; set; }
}
