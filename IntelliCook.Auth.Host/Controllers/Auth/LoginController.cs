using IntelliCook.Auth.Contract.Auth.Login;
using IntelliCook.Auth.Host.Extensions.Infrastructure;
using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace IntelliCook.Auth.Host.Controllers.Auth;

[Tags("Auth")]
[Route("Auth/[controller]")]
[ApiController]
[AllowAnonymous]
public class LoginController(UserManager<IntelliCookUser> userManager, IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    /// <summary>
    ///     Logs in a user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoginPostResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Post(LoginPostRequestModel request)
    {
        var user = await userManager.FindByNameAsync(request.Username);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized();
        }

        var token = user.CreateToken(jwtOptions.Value);

        return Ok(new LoginPostResponseModel
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
}