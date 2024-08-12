using FluentAssertions;
using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Host.Controllers;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace IntelliCook.Auth.Host.UnitTests.Controllers;

public class UserControllerTests
{
    private readonly UserController _userController;
    private readonly Mock<HttpContext> _httpContextMock = new();
    private readonly Mock<UserManager<IntelliCookUser>> _userManagerMock;

    private readonly IntelliCookUser _user = new()
    {
        Name = "Name",
        Role = UserRoleModel.Admin,
        UserName = "Username",
        Email = "Email@Email.com",
        PasswordHash = "Password Hash"
    };

    public UserControllerTests()
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
        _userController = new UserController(_userManagerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContextMock.Object
            }
        };
    }

    #region Get

    [Fact]
    public async void Get_Success_ReturnsOkObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });
        _userManagerMock
            .Setup(m => m.FindByNameAsync(_user.Name))
            .ReturnsAsync(_user);

        // Act
        var result = await _userController.Get();

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which
            .Value.Should().BeOfType<UserGetResponseModel>().Which
            .Should().BeEquivalentTo(new UserGetResponseModel
            {
                Name = _user.Name,
                Role = _user.Role,
                Username = _user.UserName,
                Email = _user.Email
            });
    }

    [Fact]
    public async void Get_NameNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });

        // Act
        var result = await _userController.Get();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async void Get_RoleNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name)
            });

        // Act
        var result = await _userController.Get();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async void Get_UserNotFound_ReturnsNotFoundObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });
        _userManagerMock
            .Setup(m => m.FindByNameAsync(_user.Name))
            .ReturnsAsync(null as IntelliCookUser);

        // Act
        var result = await _userController.Get();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}