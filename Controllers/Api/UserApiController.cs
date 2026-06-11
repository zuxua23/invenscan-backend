using InvenScan.DTO.Request;
using InvenScan.Service.Interfaces;
using InvenScan.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenScan.Controllers.Api;

[ApiController]
[Route("api/user")]
[Authorize(AuthenticationSchemes = AppConstants.AuthSchemes.Jwt, Roles = AppConstants.Roles.Admin)]
public class UserApiController : ControllerBase
{
    private readonly IUserService _userService;

    public UserApiController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Get all users.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _userService.GetAllUsersAsync();
        return Ok(response);
    }

    /// <summary>Create a new user.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserCreateRequest request)
    {
        var response = await _userService.CreateUserAsync(request);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>Update a user by userId.</summary>
    [HttpPut("{userId}")]
    public async Task<IActionResult> Update(string userId, [FromBody] UserUpdateRequest request)
    {
        var response = await _userService.UpdateUserAsync(userId, request);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}
