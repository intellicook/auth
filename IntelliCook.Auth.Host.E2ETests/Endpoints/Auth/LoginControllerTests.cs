using FluentAssertions;
using IntelliCook.Auth.Host.E2ETests.Fixtures;
using IntelliCook.Auth.Host.Models.Auth.Login;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntelliCook.Auth.Host.E2ETests.Endpoints.Auth;

[Collection(nameof(ClientFixture))]
public class LoginControllerTests(ClientFixture fixture)
{
    private const string Path = "/Auth/Login";
    private readonly HttpClient _client = fixture.Client;

    #region Post

    [Fact]
    public async void Post_Success_ReturnsOkObjectResult()
    {
        // Arrange
        var request = new LoginPostRequestModel
        {
            Username = fixture.DefaultUser.UserName,
            Password = fixture.DefaultUserPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync(Path, request, fixture.SerializerOptions);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        var token = JsonSerializer.Deserialize<LoginPostResponseModel>(content, fixture.SerializerOptions);
        token.Should().NotBeNull();
        token!.AccessToken.Should().NotBeNullOrEmpty();

        // TODO: Add check to assert token is valid
    }

    [Fact]
    public async void Post_UsernameNotFound_ReturnsUnauthorizedResult()
    {
        // Arrange
        var request = new LoginPostRequestModel
        {
            Username = "Unknown Username",
            Password = fixture.DefaultUserPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync(Path, request, fixture.SerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async void Post_PasswordIncorrect_ReturnsUnauthorizedResult()
    {
        // Arrange
        var request = new LoginPostRequestModel
        {
            Username = fixture.DefaultUser.UserName,
            Password = "Incorrect Password"
        };

        // Act
        var response = await _client.PostAsJsonAsync(Path, request, fixture.SerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}