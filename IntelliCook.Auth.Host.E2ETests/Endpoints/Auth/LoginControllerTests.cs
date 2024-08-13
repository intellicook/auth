using FluentAssertions;
using IntelliCook.Auth.Contract.Auth.Login;
using IntelliCook.Auth.Host.E2ETests.Fixtures;
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
        await using var user = await fixture.GivenUser();
        var request = new LoginPostRequestModel
        {
            Username = user.Resource.username,
            Password = user.Resource.password
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
        await using var user = await fixture.GivenUser();
        var request = new LoginPostRequestModel
        {
            Username = "Unknown Username",
            Password = user.Resource.password
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
        await using var user = await fixture.GivenUser();
        var request = new LoginPostRequestModel
        {
            Username = user.Resource.username,
            Password = "Incorrect Password"
        };

        // Act
        var response = await _client.PostAsJsonAsync(Path, request, fixture.SerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}