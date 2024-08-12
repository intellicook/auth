using FluentAssertions;
using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Host.E2ETests.Fixtures;
using System.Net;
using System.Text.Json;

namespace IntelliCook.Auth.Host.E2ETests.Endpoints;

[Collection(nameof(ClientFixture))]
public class UserControllerTests(ClientFixture fixture)
{
    private const string Path = "/User";

    #region Get

    [Fact]
    public async void Get_Success_ReturnsOkObjectResult()
    {
        // Arrange
        var token = await fixture.GetToken();
        var request = new HttpRequestMessage(HttpMethod.Get, Path);
        request.Headers.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await fixture.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        var user = JsonSerializer.Deserialize<UserGetResponseModel>(content, fixture.SerializerOptions);
        user.Should().NotBeNull();
        user!.Name.Should().Be(fixture.DefaultUser.Name);
        user.Role.Should().Be(fixture.DefaultUser.Role);
        user.Username.Should().Be(fixture.DefaultUser.UserName);
        user.Email.Should().Be(fixture.DefaultUser.Email);
    }

    [Fact]
    public async void Get_TokenMissing_ReturnsUnauthorizedObjectResult()
    {
        // Act
        var response = await fixture.Client.GetAsync(Path);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}