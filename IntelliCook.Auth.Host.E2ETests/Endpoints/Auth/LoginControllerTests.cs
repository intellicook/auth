using FluentAssertions;
using IntelliCook.Auth.Contract.Auth.Login;
using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Host.E2ETests.Fixtures;
using IntelliCook.Auth.Host.E2ETests.Fixtures.Given;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntelliCook.Auth.Host.E2ETests.Endpoints.Auth;

[Collection(nameof(ClientFixture))]
public class LoginControllerTests(ClientFixture fixture)
{
    #region Post

    [Fact]
    public async void Post_Success_ReturnsOkObjectResult()
    {
        // Arrange
        await using var user = await fixture.With(new GivenUser());
        var request = new LoginPostRequestModel
        {
            Username = user.Username,
            Password = user.Password
        };

        // Act
        var result = await fixture.Client.PostAuthLogin(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Value.AccessToken.Should().NotBeNullOrEmpty();

        fixture.Client.RequestHeaders.Add("Authorization", $"Bearer {result.Value.AccessToken}");

        var meResult = await fixture.Client.GetUserMe();
        meResult.StatusCode.Should().Be(HttpStatusCode.OK);
        meResult.Value.Username.Should().Be(user.Username);

        fixture.Client.RequestHeaders.Remove("Authorization");
    }

    [Fact]
    public async void Post_UsernameNotFound_ReturnsUnauthorizedResult()
    {
        // Arrange
        await using var user = await fixture.With(new GivenUser());
        var request = new LoginPostRequestModel
        {
            Username = "Unknown Username",
            Password = user.Password
        };

        // Act
        var result = await fixture.Client.PostAuthLogin(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async void Post_PasswordIncorrect_ReturnsUnauthorizedResult()
    {
        // Arrange
        await using var user = await fixture.With(new GivenUser());
        var request = new LoginPostRequestModel
        {
            Username = user.Username,
            Password = "Incorrect Password"
        };

        // Act
        var result = await fixture.Client.PostAuthLogin(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}