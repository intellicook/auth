using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Host.Extensions;
using IntelliCook.Auth.Host.Extensions.Infrastructure;
using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IntelliCook.Auth.Host.Controllers.User;

[Tags("User")]
[Route("User/[controller]")]
[ApiController]
[Authorize]
public class MeController(UserManager<IntelliCookUser> userManager, IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    /// <summary>
    ///     Gets the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(UserGetResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get()
    {
        var name = GetUsername();

        if (string.IsNullOrEmpty(name))
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

    /// <summary>
    ///     Updates the current user.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(UserPutResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(UserPutRequestModel request)
    {
        var name = GetUsername();

        if (name == null)
        {
            return BadRequest(this.CreateValidationProblemDetails());
        }

        var user = await userManager.FindByNameAsync(name);

        if (user == null)
        {
            return NotFound(this.CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "User not found",
                detail: "Invalid token with no user found."
            ));
        }

        user.Name = request.Name;
        user.UserName = request.Username;
        user.Email = request.Email;

        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            var token = user.CreateToken(jwtOptions.Value);
            return Ok(new UserPutResponseModel
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
        {
            ModelState.AddModelError(nameof(request.Username), "Username is already taken.");
        }

        if (result.Errors.Any(e => e.Code == "DuplicateEmail"))
        {
            ModelState.AddModelError(nameof(request.Email), "Email is already taken.");
        }

        foreach (var error in result.Errors.Where(e => e.Code != "DuplicateUserName" && e.Code != "DuplicateEmail"))
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(this.CreateValidationProblemDetails());
    }

    /// <summary>
    ///     Updates the current user's password.
    /// </summary>
    [Route("Password")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutPassword(UserPasswordPutRequestModel request)
    {
        var name = GetUsername();

        if (name == null)
        {
            return BadRequest(this.CreateValidationProblemDetails());
        }

        var user = await userManager.FindByNameAsync(name);

        if (user == null)
        {
            return NotFound(this.CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "User not found",
                detail: "Invalid token with no user found."
            ));
        }

        var result = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        if (result.Succeeded)
        {
            return NoContent();
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(this.CreateValidationProblemDetails());
    }

    /// <summary>
    ///     Deletes the current user.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete()
    {
        var name = GetUsername();

        if (name == null)
        {
            return BadRequest(this.CreateValidationProblemDetails());
        }

        var user = await userManager.FindByNameAsync(name);

        if (user == null)
        {
            return NotFound(this.CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "User not found",
                detail: "Invalid token with no user found."
            ));
        }

        var result = await userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            return NoContent();
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(this.CreateValidationProblemDetails());
    }

    private string? GetUsername()
    {
        var name = User.Claims
            .SingleOrDefault(c => c.Type == ClaimTypes.Name)?
            .Value;

        if (name == null)
        {
            ModelState.AddModelError(nameof(ClaimTypes.Name), "Invalid token with no name is given.");
        }

        var email = User.Claims
            .SingleOrDefault(c => c.Type == ClaimTypes.Email)?
            .Value;

        if (email == null)
        {
            ModelState.AddModelError(nameof(ClaimTypes.Email), "Invalid token with no email is given.");
        }

        var role = User.Claims
            .SingleOrDefault(c => c.Type == ClaimTypes.Role)?
            .Value;

        if (role == null)
        {
            ModelState.AddModelError(nameof(ClaimTypes.Role), "Invalid token with no role is given.");
        }

        if (name == null || email == null || role == null)
        {
            return null;
        }

        return name;
    }
}