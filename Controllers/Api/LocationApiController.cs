using InvenScan.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenScan.Controllers.Api;

[ApiController]
[Route("api/location")]
[Authorize]
public class LocationApiController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationApiController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>Get all active locations.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _locationService.GetAllLocationsAsync();
        return Ok(response);
    }
}
