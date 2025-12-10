using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpPost("register")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> Register([FromBody] UserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _userService.RegisterAsync(dto);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _userService.ChangePasswordAsync(dto);
        return NoContent();
    }

    [HttpPost("generate-password")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> GeneratePassword([FromBody] string email)
    {
        await _userService.GeneratePasswordAsync(email);
        return NoContent();
    }

    [HttpGet("all")]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetUser()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userService.GetByIdAsync(userId);
        return Ok(user);
    }

    [HttpDelete("{email}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> Delete([FromRoute] string email)
    {
        await _userService.DeleteAsync(email);
        return NoContent();
    }
}
