namespace InvenScan.DTO.Request;

public class TagRegisterRequest
{
    public List<TagRegisterItem> Tags { get; set; } = new();
}

public class TagRegisterItem
{
    public string TagId { get; set; } = string.Empty;
    public string EpcTag { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public int LocationId { get; set; }
}
