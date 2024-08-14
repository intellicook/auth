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
    private const string Path = "/Auth/Login";
    private readonly HttpClient _client = fixture.Client;

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
        var response = await _client.PostAsJsonAsync(Path, request, fixture.SerializerOptions);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        var token = JsonSerializer.Deserialize<LoginPostResponseModel>(content, fixture.SerializerOptions);
        token.Should().NotBeNull();
        token!.AccessToken.Should().NotBeNullOrEmpty();

        var meRequest = new HttpRequestMessage(HttpMethod.Get, "/User/Me");
        meRequest.Headers.Add("Authorization", $"Bearer {token.AccessToken}");

        var meResponse = await _client.SendAsync(meRequest);
        meResponse.EnsureSuccessStatusCode();

        var meContent = await meResponse.Content.ReadAsStringAsync();
        meContent.Should().NotBeNullOrEmpty();

        var meUserResponse = JsonSerializer.Deserialize<UserGetResponseModel>(meContent, fixture.SerializerOptions);
        meUserResponse.Should().NotBeNull();
        meUserResponse!.Username.Should().Be(user.Username);
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
        var response = await _client.PostAsJsonAsync(Path, request, fixture.SerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
        var response = await _client.PostAsJsonAsync(Path, request, fixture.SerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}