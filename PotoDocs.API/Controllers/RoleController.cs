using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;

namespace PotoDocs.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RoleController(IRoleService roleService) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;

    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<string>>> GetRoles()
    {
        var roles = await _roleService.GetRolesAsync();
        return Ok(roles);
    }
}