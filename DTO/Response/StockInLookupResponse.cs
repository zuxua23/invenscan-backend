namespace InvenScan.DTO.Response;

public class StockInLookupResponse
{
    public string ScannedCode { get; set; } = string.Empty;
    public string ScanType { get; set; } = string.Empty;
    public int? TagId { get; set; }
    public string? EpcTag { get; set; }
    public int ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string TagStatus { get; set; } = string.Empty;
}
