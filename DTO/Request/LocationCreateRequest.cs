namespace InvenScan.DTO.Request;

public class LocationCreateRequest
{
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
