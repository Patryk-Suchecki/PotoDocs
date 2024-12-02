using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;
using System.Net;

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
        if (!ModelState.IsValid)
        {
            BadRequest(ModelState);
        }
        var response = _accountService.RegisterUser(dto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            BadRequest(ModelState);
        }
        var response = await _accountService.LoginAsync(dto, cancellationToken);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("change-password")]
    [Authorize]
    public ActionResult ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            BadRequest(ModelState);
        }
        var response = _accountService.ChangePassword(dto);
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("all")]
    [Authorize]
    public ActionResult<IEnumerable<UserDto>> GetUsers()
    {
        var response = _accountService.GetAll();
        return StatusCode(response.StatusCode, response);
    }
    [HttpGet("all/roles")]
    [Authorize]
    public ActionResult<IEnumerable<string>> GetRoles()
    {
        var response = _accountService.GetRoles();
        return StatusCode(response.StatusCode, response);
    }
}
