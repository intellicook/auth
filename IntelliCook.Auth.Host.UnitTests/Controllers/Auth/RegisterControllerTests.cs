using FluentAssertions;
using IntelliCook.Auth.Host.Controllers.Auth;
using IntelliCook.Auth.Host.Models.Auth.Register;
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

    public static IEnumerable<object[]> Post_InvalidName_ReturnsBadRequest_TestData()
    {
        yield return
        [
            "",
            new List<string> { "Name cannot be empty." }
        ];
        yield return
        [
            new string('a', 257),
            new List<string> { "Name cannot be longer than 256 characters." }
        ];
    }

    [Theory]
    [MemberData(nameof(Post_InvalidName_ReturnsBadRequest_TestData))]
    public async void Post_InvalidName_ReturnsBadRequest(
        string name,
        IList<string> expectedErrors
    )
    {
        // Arrange
        var request = new RegisterPostRequestModel
        {
            Name = name,
            Username = "Username",
            Email = "Email@Email.com",
            Password = "Password"
        };

        // Act
        var result = await _registerController.Post(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<RegisterPostBadRequestResponseModel>().Which
            .Name.Should().BeEquivalentTo(expectedErrors);

        _userManagerMock.Verify(m => m.CreateAsync(
            It.IsAny<IntelliCookUser>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    public static IEnumerable<object[]> Post_InvalidUsername_ReturnsBadRequest_TestData()
    {
        yield return
        [
            "",
            new List<string> { "Username cannot be empty." }
        ];
        yield return
        [
            new string('a', 257),
            new List<string> { "Username cannot be longer than 256 characters." }
        ];
    }

    [Theory]
    [MemberData(nameof(Post_InvalidUsername_ReturnsBadRequest_TestData))]
    public async void Post_InvalidUsername_ReturnsBadRequest(
        string username,
        IList<string> expectedErrors
    )
    {
        // Arrange
        var request = new RegisterPostRequestModel
        {
            Name = "Name",
            Username = username,
            Email = "Email@Email.com",
            Password = "Password"
        };

        // Act
        var result = await _registerController.Post(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<RegisterPostBadRequestResponseModel>().Which
            .Username.Should().BeEquivalentTo(expectedErrors);

        _userManagerMock.Verify(m => m.CreateAsync(
            It.IsAny<IntelliCookUser>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    public static IEnumerable<object[]> Post_InvalidEmail_ReturnsBadRequest_TestData()
    {
        yield return
        [
            "",
            new List<string> { "Email cannot be empty." }
        ];
        yield return
        [
            new string('a', 257),
            new List<string> { "Email cannot be longer than 256 characters." }
        ];
        yield return
        [
            "invalid",
            new List<string> { new IdentityErrorDescriber().InvalidEmail("invalid").Description }
        ];
    }

    [Theory]
    [MemberData(nameof(Post_InvalidEmail_ReturnsBadRequest_TestData))]
    public async void Post_InvalidEmail_ReturnsBadRequest(
        string email,
        IList<string> expectedErrors
    )
    {
        // Arrange
        var request = new RegisterPostRequestModel
        {
            Name = "Name",
            Username = "Username",
            Email = email,
            Password = "Password"
        };

        // Act
        var result = await _registerController.Post(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<RegisterPostBadRequestResponseModel>().Which
            .Email.Should().BeEquivalentTo(expectedErrors);

        _userManagerMock.Verify(m => m.CreateAsync(
            It.IsAny<IntelliCookUser>(),
            It.IsAny<string>()
        ), Times.Never);
    }

    [Fact]
    public async void Post_UserManagerCreateAsyncFails_ReturnsBadRequest()
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
                new IdentityError { Code = "Name", Description = "Name Error" },
                new IdentityError { Code = "UserName", Description = "Username Error" },
                new IdentityError { Code = "Email", Description = "Email Error" },
                new IdentityError { Code = "Password", Description = "Password Error" }
            ));

        // Act
        var result = await _registerController.Post(request);

        // Assert
        var response = result.Should().BeOfType<BadRequestObjectResult>().Which
            .Value.Should().BeOfType<RegisterPostBadRequestResponseModel>().Subject;

        response.Name.Should().BeEquivalentTo("Name Error");
        response.Username.Should().BeEquivalentTo("Username Error");
        response.Email.Should().BeEquivalentTo("Email Error");
        response.Password.Should().BeEquivalentTo("Password Error");

        _userManagerMock.Verify(m => m.CreateAsync(
            It.IsAny<IntelliCookUser>(),
            It.IsAny<string>()
        ), Times.Once);
    }

    #endregion
}