using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;
using PotoDocs.Shared.Models;

namespace PotoDocs.API.Controllers;

[Route("api/account")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("register")]
    public ActionResult RegisterUser([FromBody]RegisterUserDto dto)
    {
        _accountService.RegisterUser(dto);
        return Ok();
    }

    [HttpPost("login")]
    public ActionResult Login([FromBody]LoginDto dto)
    {
        var loginResponse = _accountService.GenerateJwt(dto);
        return Ok(loginResponse);
    }

    [HttpPost("change-password")]
    public ActionResult ChangePassword([FromBody] ChangePasswordDto dto)
    {
        _accountService.ChangePassword(dto);
        return Ok();
    }
}
