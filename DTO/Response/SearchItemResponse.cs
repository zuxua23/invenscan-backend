namespace InvenScan.DTO.Response;

public class SearchItemResponse
{
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int MinStock { get; set; }
    public List<TagSummary> Tags { get; set; } = new();
}

public class TagSummary
{
    public int TagId { get; set; }
    public string EpcTag { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
}
