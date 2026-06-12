namespace InvenScan.Entity;

public class GateLog
{
    public int Id { get; set; }
    public int GateConfigId { get; set; }
    public string EpcTag { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string RawPayload { get; set; } = string.Empty;
    public string Status { get; set; } = "PROCESSED";
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    public GateConfig GateConfig { get; set; } = null!;
}
