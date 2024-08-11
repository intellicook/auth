using IntelliCook.Auth.Host.Models.Auth.Register;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace IntelliCook.Auth.Host.Controllers.Auth;

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
    [ProducesResponseType(typeof(RegisterPostBadRequestResponseModel), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(RegisterPostRequestModel request)
    {
        // Validation
        {
            var badRequestResponse = new RegisterPostBadRequestResponseModel();

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                badRequestResponse.Name.Add("Name cannot be empty.");
            }
            else if (request.Name.Length > 256)
            {
                badRequestResponse.Name.Add("Name cannot be longer than 256 characters.");
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                badRequestResponse.Username.Add("Username cannot be empty.");
            }
            else if (request.Username.Length > 256)
            {
                badRequestResponse.Username.Add("Username cannot be longer than 256 characters.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                badRequestResponse.Email.Add("Email cannot be empty.");
            }
            else if (request.Email.Length > 256)
            {
                badRequestResponse.Email.Add("Email cannot be longer than 256 characters.");
            }
            else if (!new EmailAddressAttribute().IsValid(request.Email))
            {
                badRequestResponse.Email.Add(_userManager.ErrorDescriber.InvalidEmail(request.Email).Description);
            }

            if (badRequestResponse.Email.Any() || badRequestResponse.Name.Any() || badRequestResponse.Username.Any() ||
                badRequestResponse.Password.Any())
            {
                return BadRequest(badRequestResponse);
            }
        }

        // Try to create the user
        var user = new IntelliCookUser
        {
            Name = request.Name
        };
        await _userStore.SetUserNameAsync(user, request.Username, CancellationToken.None);
        await _userEmailStore.SetEmailAsync(user, request.Email, CancellationToken.None);
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Aggregate(new RegisterPostBadRequestResponseModel(), (response, e) =>
            {
                var errorType = e.Code switch
                {
                    var code when code.Contains("Password") => response.Password,
                    var code when code.Contains("UserName") => response.Username,
                    var code when code.Contains("Email") => response.Email,
                    var code when code.Contains("Name") => response.Name,
                    _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
                };

                errorType.Add(e.Description);

                return response;
            }));
        }

        return CreatedAtAction(null, null);
    }
}