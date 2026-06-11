using System.Security.Claims;
using InvenScan.DTO.Request;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenScan.Controllers.Api;

[ApiController]
[Route("api/stockin")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Jwt)]
public class StockInController : ControllerBase
{
    private readonly IStockInService _stockInService;

    public StockInController(IStockInService stockInService)
    {
        _stockInService = stockInService;
    }

    /// <summary>Lookup item/tag info by scan code.</summary>
    [HttpGet]
    public async Task<IActionResult> Lookup([FromQuery] string code, [FromQuery] string scannerType = "RFID")
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest(new { success = false, message = "code query parameter is required." });

        var response = await _stockInService.LookupScanCodeAsync(code, scannerType.ToUpper());
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>Submit a stock-in document.</summary>
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] StockInSubmitRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var response = await _stockInService.SubmitStockInAsync(request, userId);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>Bulk lookup multiple scan codes at once.</summary>
    [HttpPost("bulk-info")]
    public async Task<IActionResult> BulkInfo([FromBody] StockInBulkInfoRequest request)
    {
        var response = await _stockInService.BulkInfoAsync(request);
        return Ok(response);
    }
}
