using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotoDocs.API.Services;

namespace PotoDocs.API.Controllers;


[Route("api/[controller]")]
[ApiController]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("all")]
    [Authorize]
    public ActionResult<IEnumerable<string>> GetRoles()
    {
        var response = _roleService.GetRoles();
        return StatusCode(response.StatusCode, response);
    }
}
