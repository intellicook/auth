using FluentAssertions;
using IntelliCook.Auth.Contract.Auth.Login;
using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Host.Controllers.Auth;
using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IntelliCook.Auth.Host.UnitTests.Controllers.Auth;

public class LoginControllerTests
{
    private readonly LoginController _loginController;
    private readonly Mock<UserManager<IntelliCookUser>> _userManagerMock;
    private readonly IOptions<JwtOptions> _jwtOptions = new OptionsWrapper<JwtOptions>(new JwtOptions
    {
        Secret = "This is a secret security key, required to be 512 bits long, so here are some random words.",
        Issuer = "Issuer",
        Audience = "Audience"
    });

    private readonly IntelliCookUser _user = new()
    {
        Name = "Name",
        Role = UserRoleModel.Admin,
        UserName = "Username",
        Email = "Email@Email.com",
        PasswordHash = "Password Hash"
    };

    public LoginControllerTests()
    {
        _userManagerMock = new Mock<UserManager<IntelliCookUser>>(
            Mock.Of<IUserStore<IntelliCookUser>>(),
            null,
            null,
            null,
            null,
            null,
            new IdentityErrorDescriber(),
            null,
            null
        );
        _loginController = new LoginController(_userManagerMock.Object, _jwtOptions);
    }

    #region Post

    [Fact]
    public async void Post_Success_ReturnsOkObjectResult()
    {
        // Arrange
        var request = new LoginPostRequestModel()
        {
            Username = _user.UserName,
            Password = "Password"
        };

        _userManagerMock
            .Setup(m => m.FindByNameAsync(request.Username))
            .ReturnsAsync(_user);
        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(_user, request.Password))
            .ReturnsAsync(true);

        // Act
        var result = await _loginController.Post(request);

        // Assert
        var token = result.Should().BeOfType<OkObjectResult>().Which
            .Value.Should().BeOfType<LoginPostResponseModel>().Which
            .AccessToken;

        var securityToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        securityToken.Claims
            .SingleOrDefault(c => c.Type == ClaimTypes.Name)?
            .Value.Should().Be(_user.UserName);
        securityToken.Claims
            .SingleOrDefault(c => c.Type == ClaimTypes.Role)?
            .Value.Should().Be(_user.Role.ToString());
    }

    [Fact]
    public async void Post_UsernameNotFound_ReturnsUnauthorizedResult()
    {
        // Arrange
        var request = new LoginPostRequestModel()
        {
            Username = _user.UserName,
            Password = "Password"
        };

        _userManagerMock
            .Setup(m => m.FindByNameAsync(request.Username))
            .ReturnsAsync(null as IntelliCookUser);

        // Act
        var result = await _loginController.Post(request);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();

        _userManagerMock.Verify(m => m.CheckPasswordAsync(_user, request.Password), Times.Never);
    }

    [Fact]
    public async void Post_PasswordIncorrect_ReturnsUnauthorizedResult()
    {
        // Arrange
        var request = new LoginPostRequestModel()
        {
            Username = _user.UserName,
            Password = "Password"
        };

        _userManagerMock
            .Setup(m => m.FindByNameAsync(request.Username))
            .ReturnsAsync(_user);
        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(_user, request.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _loginController.Post(request);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    #endregion
}