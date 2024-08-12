using IntelliCook.Auth.Contract.Auth.Login;
using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IntelliCook.Auth.Host.Controllers.Auth;

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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Post(LoginPostRequestModel request)
    {
        var user = await userManager.FindByNameAsync(request.Username);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (userManager.SupportsUserRole)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            claims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));
        }

        var token = CreateToken(claims);

        return Ok(new LoginPostResponseModel
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }

    private JwtSecurityToken CreateToken(IEnumerable<Claim> claims)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Secret));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            jwtOptions.Value.Issuer,
            jwtOptions.Value.Audience,
            claims,
            signingCredentials: credentials
        );

        return token;
    }
}