using FluentAssertions;
using IntelliCook.Auth.Contract.Auth.Register;
using IntelliCook.Auth.Host.Controllers.Auth;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IntelliCook.Auth.Host.UnitTests.Controllers.Auth;

public class RegisterControllerTests
{
    private readonly RegisterController _registerController;
    private readonly Mock<UserManager<IntelliCookUser>> _userManagerMock;
    private readonly Mock<IUserEmailStore<IntelliCookUser>> _userStoreMock = new();

    public RegisterControllerTests()
    {
        _userManagerMock = new Mock<UserManager<IntelliCookUser>>(
            _userStoreMock.Object,
            null,
            null,
            null,
            null,
            null,
            new IdentityErrorDescriber(),
            null,
            null
        );
        _userManagerMock.Setup(m => m.SupportsUserEmail).Returns(true);
        _registerController = new RegisterController(_userManagerMock.Object, _userStoreMock.Object);
    }

    #region Post

    [Fact]
    public async void Post_Valid_ReturnsCreatedResult()
    {
        // Arrange
        var request = new RegisterPostRequestModel
        {
            Name = "Name",
            Username = "Username",
            Email = "Email@Email.com",
            Password = "Password"
        };

        _userStoreMock
            .Setup(m => m.SetUserNameAsync(
                It.IsAny<IntelliCookUser>(),
                request.Username,
                It.IsAny<CancellationToken>()
            ))
            .Callback((IntelliCookUser user, string username, CancellationToken _) => user.UserName = username);
        _userStoreMock
            .Setup(m => m.SetEmailAsync(
                It.IsAny<IntelliCookUser>(),
                request.Email,
                It.IsAny<CancellationToken>()
            ))
            .Callback((IntelliCookUser user, string email, CancellationToken _) => user.Email = email);
        _userManagerMock
            .Setup(m => m.CreateAsync(
                It.Is<IntelliCookUser>(user =>
                    user.Name == request.Name &&
                    user.UserName == request.Username &&
                    user.Email == request.Email
                ),
                request.Password
            ))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _registerController.Post(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        _userManagerMock.Verify(m => m.CreateAsync(
            It.Is<IntelliCookUser>(user =>
                user.Name == request.Name &&
                user.UserName == request.Username &&
                user.Email == request.Email
            ),
            request.Password
        ), Times.Once);
    }

    [Fact]
    public async void Post_DuplicateEmail_ReturnsValidationProblemDetails()
    {
        // Arrange
        var request = new RegisterPostRequestModel
        {
            Name = "Name",
            Username = "Username",
            Email = "Email@Email.com",
            Password = "Password"
        };

        _userManagerMock
            .Setup(m => m.CreateAsync(
                It.IsAny<IntelliCookUser>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "DuplicateEmail", Description = "Email Error" }
            ));

        // Act
        var result = await _registerController.Post(request);

        // Assert
        var response = result.Should().BeOfType<ObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>().Subject;

        response.Errors.Should().ContainKey(nameof(request.Email));
        response.Errors[nameof(request.Email)].Should().Contain("Email is already taken.");

        _userManagerMock.Verify(m => m.CreateAsync(
            It.IsAny<IntelliCookUser>(),
            It.IsAny<string>()
        ), Times.Once);
    }

    [Fact]
    public async void Post_DuplicateUsername_ReturnsValidationProblemDetails()
    {
        // Arrange
        var request = new RegisterPostRequestModel
        {
            Name = "Name",
            Username = "Username",
            Email = "Email@Email.com",
            Password = "Password"
        };

        _userManagerMock
            .Setup(m => m.CreateAsync(
                It.IsAny<IntelliCookUser>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "DuplicateUserName", Description = "Username Error" }
            ));

        // Act
        var result = await _registerController.Post(request);

        // Assert
        var response = result.Should().BeOfType<ObjectResult>().Which
            .Value.Should().BeOfType<ValidationProblemDetails>().Subject;

        response.Errors.Should().ContainKey(nameof(request.Username));
        response.Errors[nameof(request.Username)].Should().Contain("Username is already taken.");

        _userManagerMock.Verify(m => m.CreateAsync(
            It.IsAny<IntelliCookUser>(),
            It.IsAny<string>()
        ), Times.Once);
    }

    #endregion
}