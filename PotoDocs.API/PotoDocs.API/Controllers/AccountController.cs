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
}
