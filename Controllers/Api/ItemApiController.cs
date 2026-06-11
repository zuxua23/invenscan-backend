using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenScan.Controllers.Api;

[ApiController]
[Route("api/item")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Jwt)]
public class ItemApiController : ControllerBase
{
    private readonly IItemService _itemService;

    public ItemApiController(IItemService itemService)
    {
        _itemService = itemService;
    }

    /// <summary>Get all active items.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _itemService.GetAllItemsAsync();
        return Ok(response);
    }

    /// <summary>Get item by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _itemService.GetItemByIdAsync(id);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}
