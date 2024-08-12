using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Host.Extensions;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IntelliCook.Auth.Host.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class UserController(UserManager<IntelliCookUser> userManager) : ControllerBase
{
    /// <summary>
    ///     Gets the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(UserGetResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get()
    {
        var name = User.Claims
            .SingleOrDefault(c => c.Type == ClaimTypes.Name)?
            .Value;

        if (name == null)
        {
            ModelState.AddModelError(nameof(ClaimTypes.Name), "Invalid token with no name is given.");
        }

        var role = User.Claims
            .SingleOrDefault(c => c.Type == ClaimTypes.Role)?
            .Value;

        if (role == null)
        {
            ModelState.AddModelError(nameof(ClaimTypes.Role), "Invalid token with no role is given.");
        }

        if (name == null || role == null)
        {
            return BadRequest(this.CreateValidationProblemDetails());
        }

        var user = await userManager.FindByNameAsync(name);

        if (user != null)
        {
            return Ok(new UserGetResponseModel()
            {
                Name = user.Name,
                Role = user.Role,
                Username = user.UserName,
                Email = user.Email
            });
        }

        return NotFound(this.CreateProblemDetails(
            StatusCodes.Status404NotFound,
            "User not found",
            detail: "Invalid token with no user found."
        ));
    }
}