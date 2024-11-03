using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Controllers;

[Route("api/[controller]")]
[ApiController]

public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    
    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("register")]
    [Authorize(Roles = "admin,manager")]
    public ActionResult RegisterUser([FromBody]UserDto dto)
    {
        _accountService.RegisterUser(dto);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var response = await _accountService.LoginAsync(dto, cancellationToken);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("change-password")]
    [Authorize]
    public ActionResult ChangePassword([FromBody] ChangePasswordDto dto)
    {
        _accountService.ChangePassword(dto);
        return Ok();
    }
    [HttpGet("all")]
    [Authorize]
    public ActionResult<IEnumerable<UserDto>> GetUsers()
    {
        var response = _accountService.GetAll();
        return StatusCode(response.StatusCode, response);
    }
}
