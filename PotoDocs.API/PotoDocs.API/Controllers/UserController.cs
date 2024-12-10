using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    [Authorize(Roles = "admin,manager")]
    public ActionResult Create([FromBody] UserDto dto)
    {
        if (!ModelState.IsValid)
        {
            BadRequest(ModelState);
        }
        var response = _userService.RegisterUser(dto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("change-password")]
    public ActionResult ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            BadRequest(ModelState);
        }
        var response = _userService.ChangePassword(dto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("generate-password")]
    public ActionResult GeneratePassword([FromBody] string email)
    {
        var response = _userService.GeneratePassword(email);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("all")]
    public ActionResult<IEnumerable<UserDto>> GetUsers()
    {
        var response = _userService.GetAll();
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet]
    public ActionResult<IEnumerable<string>> GetUser()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var response = _userService.GetUser(userId);
        return StatusCode(response.StatusCode, response);
    }
}
