using InvenScan.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenScan.Controllers.Api;

[ApiController]
[Route("api/search-item")]
[Authorize]
public class SearchItemController : ControllerBase
{
    private readonly ISearchItemService _searchItemService;

    public SearchItemController(ISearchItemService searchItemService)
    {
        _searchItemService = searchItemService;
    }

    /// <summary>Get all items with tag info for device cache.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _searchItemService.GetAllItemsWithTagsAsync();
        return Ok(response);
    }

    /// <summary>Get item detail by ItemCode or EPC tag.</summary>
    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var response = await _searchItemService.GetByCodeAsync(code);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}
