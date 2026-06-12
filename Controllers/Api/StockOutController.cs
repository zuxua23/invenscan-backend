using System.Security.Claims;
using InvenScan.DTO.Request;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenScan.Controllers.Api;

[ApiController]
[Route("api/stockout")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Jwt)]
public class StockOutController : ControllerBase
{
    private readonly IStockOutService _stockOutService;

    public StockOutController(IStockOutService stockOutService)
    {
        _stockOutService = stockOutService;
    }

    /// <summary>Lookup item/tag info by scan code.</summary>
    [HttpGet]
    public async Task<IActionResult> Lookup([FromQuery] string code, [FromQuery] string scannerType = "RFID")
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest(new { success = false, message = "code query parameter is required." });

        var response = await _stockOutService.LookupScanCodeAsync(code, scannerType.ToUpper());
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>Submit a stock-out document.</summary>
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] StockOutSubmitRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var response = await _stockOutService.SubmitStockOutAsync(request, userId);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>Bulk lookup multiple scan codes.</summary>
    [HttpPost("bulk-info")]
    public async Task<IActionResult> BulkInfo([FromBody] BulkInfoRequest request)
    {
        var response = await _stockOutService.BulkInfoAsync(request.Codes, request.ScannerType);
        return Ok(response);
    }
}

public class BulkInfoRequest
{
    public string[] Codes { get; set; } = Array.Empty<string>();
    public string ScannerType { get; set; } = "RFID";
}
