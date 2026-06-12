namespace InvenScan.Entity;

public class GateConfig
{
    public int Id { get; set; }
    public string GateName { get; set; } = string.Empty;
    public string GateCode { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string FieldMapping { get; set; } = "{\"epc\":\"epc\"}";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Location? Location { get; set; }
    public ICollection<GateLog> Logs { get; set; } = new List<GateLog>();
}
