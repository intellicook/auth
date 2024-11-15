using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntelliCook.Auth.Host.Controllers.Admin;

[Tags("Admin")]
[Route("Admin/[controller]")]
[ApiController]
[Authorize(Policy = "Admin")]
public class UsersController(UserManager<IntelliCookUser> userManager) : ControllerBase
{
    /// <summary>
    ///     Get a list of all users.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserGetResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get()
    {
        var users = await userManager.Users.ToListAsync();

        return Ok(users.Select(user => new UserGetResponseModel
        {
            Name = user.Name,
            Role = user.Role,
            Username = user.UserName,
            Email = user.Email
        }));
    }
}