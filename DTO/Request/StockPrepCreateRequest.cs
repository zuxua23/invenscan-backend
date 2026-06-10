namespace InvenScan.DTO.Request;

public class StockPrepCreateRequest
{
    public string Notes { get; set; } = string.Empty;
    public List<StockPrepDetailCreateItem> Items { get; set; } = new();
}

public class StockPrepDetailCreateItem
{
    public int ItemId { get; set; }
    public int LocationId { get; set; }
    public int RequestedQty { get; set; }
}
