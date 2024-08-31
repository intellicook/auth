using FluentAssertions;
using IntelliCook.Auth.Client.E2ETests.Fixtures;
using IntelliCook.Auth.Client.E2ETests.Fixtures.Given;
using IntelliCook.Auth.Contract.Auth.Login;
using IntelliCook.Auth.Contract.User;
using System.Net;

namespace IntelliCook.Auth.Client.E2ETests.Endpoints.User;

[Collection(nameof(ClientFixture))]
public class MeControllerTests(ClientFixture fixture)
{
    #region Get

    [Fact]
    public async void Get_Success_ReturnsOkObjectResult()
    {
        // Arrange
        await using var user = await fixture.With(new GivenUser(true));

        // Act
        var result = await fixture.Client.GetUserMeAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Value.Should().BeEquivalentTo(new UserGetResponseModel
        {
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            Role = UserRoleModel.User
        });
    }

    [Fact]
    public async void Get_TokenMissing_ReturnsUnauthorizedResult()
    {
        // Act
        var result = await fixture.Client.GetUserMeAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Put

    [Fact]
    public async void Put_Success_ReturnsNoContentResult()
    {
        // Arrange
        await using var user = await fixture.With(new GivenUser(true), false);
        var request = new UserPutRequestModel
        {
            Name = "New Name",
            Username = "New_Username",
            Email = "New@Email.com"
        };

        // Act
        var result = await fixture.Client.PutUserMeAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var oldMeResult = await fixture.Client.GetUserMeAsync();
        oldMeResult.StatusCode.Should().Be(HttpStatusCode.NotFound);

        user.Name = request.Name;
        user.Username = request.Username;
        user.Email = request.Email;
        fixture.Client.RequestHeaders.Remove("Authorization");
        fixture.Client.RequestHeaders.Add("Authorization", $"Bearer {await user.GetToken()}");

        var meResult = await fixture.Client.GetUserMeAsync();
        meResult.StatusCode.Should().Be(HttpStatusCode.OK);
        meResult.Value.Should().BeEquivalentTo(new UserGetResponseModel
        {
            Name = request.Name,
            Role = UserRoleModel.User,
            Username = request.Username,
            Email = request.Email
        });

        // Cleanup
        await user.Cleanup();
    }

    [Fact]
    public async void Put_TokenMissing_ReturnsUnauthorizedResult()
    {
        // Arrange
        var request = new UserPutRequestModel
        {
            Name = "New Name",
            Username = "New_Username",
            Email = "New@Email.com"
        };

        // Act
        var response = await fixture.Client.PutUserMeAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region PutPassword

    [Fact]
    public async void PutPassword_Success_ReturnsNoContentResult()
    {
        // Arrange
        await using var user = await fixture.With(new GivenUser(true), false);

        var request = new UserPasswordPutRequestModel
        {
            OldPassword = user.Password,
            NewPassword = "New Password 1234"
        };

        // Act
        var result = await fixture.Client.PutUserMePasswordAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        user.Password = request.NewPassword;
        fixture.Client.RequestHeaders.Remove("Authorization");
        fixture.Client.RequestHeaders.Add("Authorization", $"Bearer {await user.GetToken()}");

        var loginRequest = new LoginPostRequestModel
        {
            Username = user.Username,
            Password = request.NewPassword
        };

        var loginResult = await fixture.Client.PostAuthLoginAsync(loginRequest);
        loginResult.StatusCode.Should().Be(HttpStatusCode.OK);

        var oldLoginRequest = new LoginPostRequestModel
        {
            Username = user.Username,
            Password = request.OldPassword
        };

        var oldLoginResult = await fixture.Client.PostAuthLoginAsync(oldLoginRequest);
        oldLoginResult.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Cleanup
        await user.Cleanup();
    }

    [Fact]
    public async void PutPassword_TokenMissing_ReturnsUnauthorizedResult()
    {
        // Arrange
        var request = new UserPasswordPutRequestModel
        {
            OldPassword = "Default Password 1234",
            NewPassword = "New Password 1234"
        };

        // Act
        var response = await fixture.Client.PutUserMePasswordAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Delete

    [Fact]
    public async void Delete_Success_ReturnsNoContentResult()
    {
        // Arrange
        await using var user = await fixture.With(new GivenUser(true), false);

        // Act
        var result = await fixture.Client.DeleteUserMeAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var meResult = await fixture.Client.GetUserMeAsync();
        meResult.StatusCode.Should().Be(HttpStatusCode.NotFound);

        fixture.Client.RequestHeaders.Remove("Authorization");
    }

    [Fact]
    public async void Delete_TokenMissing_ReturnsUnauthorizedResult()
    {
        // Act
        var response = await fixture.Client.DeleteUserMeAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}