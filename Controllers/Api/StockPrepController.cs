using System.Security.Claims;
using InvenScan.DTO.Request;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenScan.Controllers.Api;

[ApiController]
[Route("api/stockprep")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Jwt)]
public class StockPrepController : ControllerBase
{
    private readonly IStockPrepService _stockPrepService;

    public StockPrepController(IStockPrepService stockPrepService)
    {
        _stockPrepService = stockPrepService;
    }

    /// <summary>Get all open/in-progress picking lists.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _stockPrepService.GetOpenDocumentsAsync();
        return Ok(response);
    }

    /// <summary>Get picking list detail with items.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _stockPrepService.GetByIdAsync(id);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>Submit picked items from handheld device.</summary>
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkPick([FromBody] StockPrepBulkRequest request)
    {
        var response = await _stockPrepService.BulkPickAsync(request);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
