using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IntelliCook.Auth.Host.Controllers;

// TODO: Remove this controller and replace with actual implementation.

[Route("[controller]")]
[ApiController]
[Authorize]
public class UserController(UserManager<IntelliCookUser> userManager) : ControllerBase
{
    /// <summary>
    ///     Gets the current user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var name = User.Claims
            .Where(c => c.Type == ClaimTypes.Name)
            .Select(c => c.Value)
            .FirstOrDefault();

        if (name == null)
        {
            return Unauthorized();
        }

        var user = await userManager.FindByNameAsync(name);
        return Ok(new
        {
            Name = user?.Name,
            Email = user?.Email,
            Username = user?.UserName
        });
    }
}