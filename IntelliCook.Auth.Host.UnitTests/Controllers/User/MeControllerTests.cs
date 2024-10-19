using FluentAssertions;
using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Host.Controllers.User;
using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IntelliCook.Auth.Host.UnitTests.Controllers.User;

public class MeControllerTests
{
    private readonly MeController _meController;
    private readonly Mock<HttpContext> _httpContextMock = new();
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

    private readonly UserPutRequestModel _userPutRequest = new()
    {
        Name = "New Name",
        Username = "New_Username",
        Email = "New@Email.com"
    };

    public MeControllerTests()
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
        _meController = new MeController(_userManagerMock.Object, _jwtOptions)
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
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });
        _userManagerMock
            .Setup(m => m.FindByNameAsync(_user.Name))
            .ReturnsAsync(_user);

        // Act
        var result = await _meController.Get();

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
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });

        // Act
        var result = await _meController.Get();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async void Get_EmailNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });

        // Act
        var result = await _meController.Get();

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
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Email, _user.Email)
            });

        // Act
        var result = await _meController.Get();

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
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });
        _userManagerMock
            .Setup(m => m.FindByNameAsync(_user.Name))
            .ReturnsAsync(null as IntelliCookUser);

        // Act
        var result = await _meController.Get();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Put

    [Fact]
    public async void Put_Success_ReturnsOkObjectResult()
    {
        // Arrange
        var newUser = new IntelliCookUser
        {
            Name = _userPutRequest.Name,
            Role = _user.Role,
            UserName = _userPutRequest.Username,
            Email = _userPutRequest.Email,
            PasswordHash = _user.PasswordHash
        };

        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });
        _userManagerMock
            .Setup(m => m.FindByNameAsync(_user.Name))
            .ReturnsAsync(_user);
        _userManagerMock
            .Setup(m => m.UpdateAsync(It.Is<IntelliCookUser>(user =>
                user.Name == newUser.Name &&
                user.Role == newUser.Role &&
                user.UserName == newUser.UserName &&
                user.Email == newUser.Email &&
                user.PasswordHash == newUser.PasswordHash
            )))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _meController.Put(_userPutRequest);

        // Assert
        var token = result.Should().BeOfType<OkObjectResult>().Which
            .Value.Should().BeOfType<UserPutResponseModel>().Which
            .AccessToken;

        var securityToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        securityToken.Claims
            .SingleOrDefault(c => c.Type == ClaimTypes.Name)?
            .Value.Should().Be(_user.UserName);
        securityToken.Claims
            .SingleOrDefault(c => c.Type == ClaimTypes.Email)?
            .Value.Should().Be(_user.Email);
        securityToken.Claims
            .SingleOrDefault(c => c.Type == ClaimTypes.Role)?
            .Value.Should().Be(_user.Role.ToString());

        _userManagerMock.Verify(m => m.UpdateAsync(It.Is<IntelliCookUser>(user =>
            user.Name == newUser.Name &&
            user.Role == newUser.Role &&
            user.UserName == newUser.UserName &&
            user.Email == newUser.Email &&
            user.PasswordHash == newUser.PasswordHash
        )), Times.Once);
    }

    [Fact]
    public async void Put_UpdateFailed_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var newUser = new IntelliCookUser
        {
            Name = _userPutRequest.Name,
            Role = _user.Role,
            UserName = _userPutRequest.Username,
            Email = _userPutRequest.Email,
            PasswordHash = _user.PasswordHash
        };

        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });
        _userManagerMock
            .Setup(m => m.FindByNameAsync(_user.Name))
            .ReturnsAsync(_user);
        _userManagerMock
            .Setup(m => m.UpdateAsync(It.Is<IntelliCookUser>(user =>
                user.Name == newUser.Name &&
                user.Role == newUser.Role &&
                user.UserName == newUser.UserName &&
                user.Email == newUser.Email &&
                user.PasswordHash == newUser.PasswordHash
            )))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        // Act
        var result = await _meController.Put(_userPutRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.UpdateAsync(It.Is<IntelliCookUser>(user =>
            user.Name == newUser.Name &&
            user.Role == newUser.Role &&
            user.UserName == newUser.UserName &&
            user.Email == newUser.Email &&
            user.PasswordHash == newUser.PasswordHash
        )), Times.Once);
    }

    [Fact]
    public async void Put_NameNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });

        // Act
        var result = await _meController.Put(_userPutRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<IntelliCookUser>()), Times.Never);
    }

    [Fact]
    public async void Put_EmailNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });

        // Act
        var result = await _meController.Put(_userPutRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<IntelliCookUser>()), Times.Never);
    }

    [Fact]
    public async void Put_RoleNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Email, _user.Email)
            });

        // Act
        var result = await _meController.Put(_userPutRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<IntelliCookUser>()), Times.Never);
    }

    [Fact]
    public async void Put_UserNotFound_ReturnsNotFoundObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });
        _userManagerMock
            .Setup(m => m.FindByNameAsync(_user.Name))
            .ReturnsAsync(null as IntelliCookUser);

        // Act
        var result = await _meController.Put(_userPutRequest);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<IntelliCookUser>()), Times.Never);
    }

    #endregion

    #region PutPassword

    [Fact]
    public async void PutPassword_Success_ReturnsNoContentResult()
    {
        // Arrange
        var request = new UserPasswordPutRequestModel
        {
            OldPassword = "OldPassword",
            NewPassword = "NewPassword"
        };

        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });
        _userManagerMock
            .Setup(m => m.FindByNameAsync(_user.Name))
            .ReturnsAsync(_user);
        _userManagerMock
            .Setup(m => m.ChangePasswordAsync(_user, request.OldPassword, request.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _meController.PutPassword(request);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _userManagerMock.Verify(
            m => m.ChangePasswordAsync(_user, request.OldPassword, request.NewPassword),
            Times.Once
        );
    }

    [Fact]
    public async void PutPassword_UpdateFailed_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var request = new UserPasswordPutRequestModel
        {
            OldPassword = "OldPassword",
            NewPassword = "NewPassword"
        };

        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });
        _userManagerMock
            .Setup(m => m.FindByNameAsync(_user.Name))
            .ReturnsAsync(_user);
        _userManagerMock
            .Setup(m => m.ChangePasswordAsync(_user, request.OldPassword, request.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        // Act
        var result = await _meController.PutPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(
            m => m.ChangePasswordAsync(_user, request.OldPassword, request.NewPassword),
            Times.Once
        );
    }

    [Fact]
    public async void PutPassword_NameNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var request = new UserPasswordPutRequestModel
        {
            OldPassword = "OldPassword",
            NewPassword = "NewPassword"
        };

        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });

        // Act
        var result = await _meController.PutPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(
            m => m.ChangePasswordAsync(It.IsAny<IntelliCookUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async void PutPassword_EmailNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var request = new UserPasswordPutRequestModel
        {
            OldPassword = "OldPassword",
            NewPassword = "NewPassword"
        };

        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });

        // Act
        var result = await _meController.PutPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(
            m => m.ChangePasswordAsync(It.IsAny<IntelliCookUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async void PutPassword_RoleNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var request = new UserPasswordPutRequestModel
        {
            OldPassword = "OldPassword",
            NewPassword = "NewPassword"
        };

        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Email, _user.Email)
            });

        // Act
        var result = await _meController.PutPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(
            m => m.ChangePasswordAsync(It.IsAny<IntelliCookUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async void PutPassword_UserNotFound_ReturnsNotFoundObjectResult()
    {
        // Arrange
        var request = new UserPasswordPutRequestModel
        {
            OldPassword = "OldPassword",
            NewPassword = "NewPassword"
        };

        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });
        _userManagerMock
            .Setup(m => m.FindByNameAsync(_user.Name))
            .ReturnsAsync(null as IntelliCookUser);

        // Act
        var result = await _meController.PutPassword(request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _userManagerMock.Verify(
            m => m.ChangePasswordAsync(It.IsAny<IntelliCookUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    #endregion

    #region Delete

    [Fact]
    public async void Post_Success_ReturnsOkObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });
        _userManagerMock
            .Setup(m => m.FindByNameAsync(_user.Name))
            .ReturnsAsync(_user);
        _userManagerMock
            .Setup(m => m.DeleteAsync(_user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _meController.Delete();

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _userManagerMock.Verify(m => m.DeleteAsync(_user), Times.Once);
    }

    [Fact]
    public async void Post_DeleteFailed_ReturnsBadRequestObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });
        _userManagerMock
            .Setup(m => m.FindByNameAsync(_user.Name))
            .ReturnsAsync(_user);
        _userManagerMock
            .Setup(m => m.DeleteAsync(_user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        // Act
        var result = await _meController.Delete();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.DeleteAsync(_user), Times.Once);
    }

    [Fact]
    public async void Post_NameNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });

        // Act
        var result = await _meController.Get();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(m => m.DeleteAsync(It.IsAny<IntelliCookUser>()), Times.Never);
    }

    [Fact]
    public async void Post_EmailNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });

        // Act
        var result = await _meController.Get();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(m => m.DeleteAsync(It.IsAny<IntelliCookUser>()), Times.Never);
    }

    [Fact]
    public async void Post_RoleNotFound_ReturnsBadRequestObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Email, _user.Email)
            });

        // Act
        var result = await _meController.Get();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>();

        _userManagerMock.Verify(m => m.FindByNameAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(m => m.DeleteAsync(It.IsAny<IntelliCookUser>()), Times.Never);
    }

    [Fact]
    public async void Post_UserNotFound_ReturnsNotFoundObjectResult()
    {
        // Arrange
        _httpContextMock
            .SetupGet(m => m.User.Claims)
            .Returns(new[]
            {
                new Claim(ClaimTypes.Name, _user.Name),
                new Claim(ClaimTypes.Email, _user.Email),
                new Claim(ClaimTypes.Role, _user.Role.ToString())
            });
        _userManagerMock
            .Setup(m => m.FindByNameAsync(_user.Name))
            .ReturnsAsync(null as IntelliCookUser);

        // Act
        var result = await _meController.Get();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _userManagerMock.Verify(m => m.DeleteAsync(It.IsAny<IntelliCookUser>()), Times.Never);
    }

    #endregion
}