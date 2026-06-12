using InvenScan.Database;
using InvenScan.Entity;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace InvenScan.Service.Implementations;

public class GateService : IGateService
{
    private readonly AppDbContext _context;

    public GateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GateConfig?> ValidateApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return null;
        return await _context.GateConfigs
            .Include(g => g.Location)
            .FirstOrDefaultAsync(g => g.ApiKey == apiKey && g.IsActive);
    }

    public List<string> NormalizePayload(JsonElement raw, string fieldMapping)
    {
        var epcs = new List<string>();
        try
        {
            var mapping = JsonSerializer.Deserialize<Dictionary<string, string>>(fieldMapping)
                          ?? new Dictionary<string, string>();

            var epcFieldName = mapping.GetValueOrDefault("epc", "epc");

            if (raw.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in raw.EnumerateArray())
                    ExtractEpc(element, epcFieldName, epcs);
            }
            else if (raw.ValueKind == JsonValueKind.Object)
            {
                if (raw.TryGetProperty(epcFieldName, out var epcProp))
                {
                    if (epcProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var epc in epcProp.EnumerateArray())
                            if (epc.ValueKind == JsonValueKind.String)
                                epcs.Add(epc.GetString()!);
                    }
                    else if (epcProp.ValueKind == JsonValueKind.String)
                    {
                        epcs.Add(epcProp.GetString()!);
                    }
                }
            }
        }
        catch { }

        return epcs.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
    }

    public async Task<(int Processed, int Unknown)> ProcessGateStockOutAsync(
        GateConfig gate, List<string> epcs, string rawPayload)
    {
        if (epcs.Count == 0) return (0, 0);

        int processed = 0, unknown = 0;
        var locationId = gate.LocationId ?? 0;

        var docNumber = $"GATE-{gate.GateCode}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var stockOut = new StockOut
        {
            DocNumber = docNumber,
            LocationId = locationId,
            Notes = $"Gate Reader: {gate.GateName}",
            CreatedBy = $"GATE-{gate.GateCode}",
            Status = AppConstants.StockOutStatus.Synced
        };

        _context.StockOuts.Add(stockOut);
        await _context.SaveChangesAsync();

        foreach (var epc in epcs)
        {
            var tag = await _context.Tags
                .Include(t => t.Item)
                .FirstOrDefaultAsync(t => t.EpcTag == epc);

            if (tag != null)
            {
                _context.StockOutDetails.Add(new StockOutDetail
                {
                    StockOutId = stockOut.Id,
                    TagId = tag.Id,
                    ItemId = tag.ItemId,
                    ScannedCode = epc,
                    ScanType = AppConstants.ScanType.Rfid
                });

                tag.Status = AppConstants.TagStatus.Out;
                tag.UpdatedAt = DateTime.UtcNow;

                _context.GateLogs.Add(new GateLog
                {
                    GateConfigId = gate.Id,
                    EpcTag = epc,
                    ItemName = tag.Item.ItemName,
                    RawPayload = rawPayload,
                    Status = AppConstants.GateLogStatus.Processed,
                    ScannedAt = DateTime.UtcNow
                });

                processed++;
            }
            else
            {
                _context.GateLogs.Add(new GateLog
                {
                    GateConfigId = gate.Id,
                    EpcTag = epc,
                    ItemName = string.Empty,
                    RawPayload = rawPayload,
                    Status = AppConstants.GateLogStatus.Unknown,
                    ScannedAt = DateTime.UtcNow
                });

                unknown++;
            }
        }

        await _context.SaveChangesAsync();
        return (processed, unknown);
    }

    public async Task<List<GateLog>> GetGateLogsAsync(int gateId, DateTime? date)
    {
        var query = _context.GateLogs
            .Where(l => l.GateConfigId == gateId);

        if (date.HasValue)
            query = query.Where(l => l.ScannedAt.Date == date.Value.Date);

        return await query
            .OrderByDescending(l => l.ScannedAt)
            .Take(500)
            .ToListAsync();
    }

    public async Task<bool> VoidGateLogAsync(int logId)
    {
        try
        {
            var log = await _context.GateLogs.FindAsync(logId);
            if (log == null || log.Status != AppConstants.GateLogStatus.Processed) return false;

            log.Status = AppConstants.GateLogStatus.Void;

            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.EpcTag == log.EpcTag);
            if (tag != null)
            {
                tag.Status = AppConstants.TagStatus.InStock;
                tag.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GenerateApiKey() => Guid.NewGuid().ToString("N");

    private static void ExtractEpc(JsonElement element, string fieldName, List<string> epcs)
    {
        if (element.TryGetProperty(fieldName, out var epcProp) &&
            epcProp.ValueKind == JsonValueKind.String)
        {
            epcs.Add(epcProp.GetString()!);
        }
    }
}
