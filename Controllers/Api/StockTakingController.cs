using System.Security.Claims;
using InvenScan.DTO.Request;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenScan.Controllers.Api;

[ApiController]
[Route("api/stock-taking")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Jwt)]
public class StockTakingController : ControllerBase
{
    private readonly IStockTakingService _stockTakingService;

    public StockTakingController(IStockTakingService stockTakingService)
    {
        _stockTakingService = stockTakingService;
    }

    /// <summary>Create a new stock taking session.</summary>
    [HttpPost]
    [Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Jwt, Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] StockTakingCreateRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var response = await _stockTakingService.CreateSessionAsync(request, userId);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>Get all stock taking sessions.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _stockTakingService.GetAllSessionsAsync();
        return Ok(response);
    }

    /// <summary>Get the currently active (open) session.</summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var response = await _stockTakingService.GetActiveSessionAsync();
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>Get all tags/items belonging to a session.</summary>
    [HttpGet("tags/{sttId:int}")]
    public async Task<IActionResult> GetSessionTags(int sttId)
    {
        var response = await _stockTakingService.GetSessionTagsAsync(sttId);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>Get tags available to be added to a session (IN_STOCK, not yet in session).</summary>
    [HttpGet("available-tags/{sttId:int}")]
    public async Task<IActionResult> GetAvailableTags(int sttId)
    {
        var response = await _stockTakingService.GetAvailableTagsAsync(sttId);
        return Ok(response);
    }

    /// <summary>Submit operator scan results from handheld device.</summary>
    [HttpPost("operator-submit")]
    public async Task<IActionResult> OperatorSubmit([FromBody] StockTakingOperatorSubmitRequest request)
    {
        var operatorId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var response = await _stockTakingService.OperatorSubmitAsync(request, operatorId);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
