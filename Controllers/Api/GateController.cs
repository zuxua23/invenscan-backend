using InvenScan.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InvenScan.Controllers.Api;

[ApiController]
[Route("api/gate")]
public class GateController : ControllerBase
{
    private readonly IGateService _gateService;

    public GateController(IGateService gateService)
    {
        _gateService = gateService;
    }

    /// <summary>Receive scan events from a physical gate RFID reader.</summary>
    [HttpPost("stockout")]
    public async Task<IActionResult> GateStockOut([FromBody] JsonElement payload)
    {
        var apiKey = Request.Headers["X-Gate-Api-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
            return Unauthorized(new { success = false, message = "X-Gate-Api-Key header is required." });

        var gate = await _gateService.ValidateApiKeyAsync(apiKey);
        if (gate == null)
            return Unauthorized(new { success = false, message = "Invalid or inactive gate API key." });

        var epcs = _gateService.NormalizePayload(payload, gate.FieldMapping);
        if (epcs.Count == 0)
            return BadRequest(new { success = false, message = "No EPC tags found in payload." });

        var rawPayload = payload.GetRawText();
        var (processed, unknown) = await _gateService.ProcessGateStockOutAsync(gate, epcs, rawPayload);

        return Ok(new
        {
            success = true,
            processed,
            unknown,
            gateCode = gate.GateCode,
            message = $"Processed {processed} tag(s), {unknown} unknown."
        });
    }
}
