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
public class RegisterController(
    UserManager<IntelliCookUser> userManager,
    IUserStore<IntelliCookUser> userStore
) : ControllerBase
{
    /// <summary>
    ///     Registers a new user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RegisterPostBadRequestResponseModel), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(RegisterPostRequestModel request)
    {
        var emailStore = (IUserEmailStore<IntelliCookUser>)userStore;
        var email = request.Email;

        var emailAddressAttribute = new EmailAddressAttribute();

        // Validation
        {
            var badRequestResponse = new RegisterPostBadRequestResponseModel();

            if (!emailAddressAttribute.IsValid(email))
            {
                badRequestResponse.Email.Add(userManager.ErrorDescriber.InvalidEmail(email).Description);
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                badRequestResponse.Name.Add("Name cannot be empty.");
            }

            if (request.Name.Length > 256)
            {
                badRequestResponse.Name.Add("Name cannot be longer than 256 characters.");
            }

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                badRequestResponse.Username.Add("Username cannot be empty.");
            }

            if (request.Username.Length > 256)
            {
                badRequestResponse.Username.Add("Username cannot be longer than 256 characters.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                badRequestResponse.Email.Add("Email cannot be empty.");
            }

            if (request.Email.Length > 256)
            {
                badRequestResponse.Email.Add("Email cannot be longer than 256 characters.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                badRequestResponse.Password.Add("Password cannot be empty.");
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
        await userStore.SetUserNameAsync(user, request.Username, CancellationToken.None);
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);
        var result = await userManager.CreateAsync(user, request.Password);

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