using InvenScan.DTO.Request;
using InvenScan.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenScan.Controllers.Api;

[ApiController]
[Route("api/tag")]
[Authorize]
public class TagApiController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagApiController(ITagService tagService)
    {
        _tagService = tagService;
    }

    /// <summary>Get tag detail by EPC or TagId.</summary>
    [HttpGet("{identifier}")]
    public async Task<IActionResult> GetByIdentifier(string identifier)
    {
        var response = await _tagService.GetTagByIdentifierAsync(identifier);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>Register a list of new tags.</summary>
    [HttpPost("register")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Register([FromBody] TagRegisterRequest request)
    {
        var response = await _tagService.RegisterTagsAsync(request);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
