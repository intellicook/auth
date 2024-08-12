using IntelliCook.Auth.Contract.Auth.Register;
using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntelliCook.Auth.Host.Controllers.Auth;

[Tags("Auth")]
[Route("Auth/[controller]")]
[ApiController]
[AllowAnonymous]
public class RegisterController : ControllerBase
{
    private readonly IUserEmailStore<IntelliCookUser> _userEmailStore;
    private readonly UserManager<IntelliCookUser> _userManager;
    private readonly IUserStore<IntelliCookUser> _userStore;

    public RegisterController(UserManager<IntelliCookUser> userManager,
        IUserStore<IntelliCookUser> userStore)
    {
        _userManager = userManager;
        _userStore = userStore;

        if (!userManager.SupportsUserEmail)
        {
            throw new NotSupportedException($"{nameof(RegisterController)} requires a user store with email support.");
        }

        _userEmailStore = (IUserEmailStore<IntelliCookUser>)userStore;
    }

    /// <summary>
    ///     Registers a new user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(RegisterPostRequestModel request)
    {
        var user = new IntelliCookUser
        {
            Name = request.Name,
            Role = UserRoleModel.None
        };
        await _userStore.SetUserNameAsync(user, request.Username, CancellationToken.None);
        await _userEmailStore.SetEmailAsync(user, request.Email, CancellationToken.None);
        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            return CreatedAtAction(null, null);
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

        return BadRequest(new ValidationProblemDetails(ModelState));
    }
}