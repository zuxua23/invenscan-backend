using InvenScan.Entity;
using System.Text.Json;

namespace InvenScan.Service.Interfaces;

public interface IGateService
{
    Task<GateConfig?> ValidateApiKeyAsync(string apiKey);
    List<string> NormalizePayload(JsonElement raw, string fieldMapping);
    Task<(int Processed, int Unknown)> ProcessGateStockOutAsync(GateConfig gate, List<string> epcs, string rawPayload);
    Task<List<GateLog>> GetGateLogsAsync(int gateId, DateTime? date);
    Task<bool> VoidGateLogAsync(int logId);
    string GenerateApiKey();
}
