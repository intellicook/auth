using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IntelliCook.Auth.Host.Extensions.Infrastructure;

public static class IntelliCookUserExtensions
{
    public static JwtSecurityToken CreateToken(this IntelliCookUser user, JwtOptions options)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            options.Issuer,
            options.Audience,
            claims,
            signingCredentials: credentials
        );

        return token;
    }
}