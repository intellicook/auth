using FluentAssertions;
using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Host.E2ETests.Fixtures;
using IntelliCook.Auth.Host.E2ETests.Fixtures.Given;
using System.Net;
using System.Text.Json;

namespace IntelliCook.Auth.Host.E2ETests.Endpoints.User;

[Collection(nameof(ClientFixture))]
public class MeControllerTests(ClientFixture fixture)
{
    private const string Path = "/User/Me";

    #region Get

    [Fact]
    public async void Get_Success_ReturnsOkObjectResult()
    {
        // Arrange
        await using var user = await fixture.With(new GivenUser());
        var token = await user.GetToken();
        var request = new HttpRequestMessage(HttpMethod.Get, Path);
        request.Headers.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await fixture.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        var userResponse = JsonSerializer.Deserialize<UserGetResponseModel>(content, fixture.SerializerOptions);
        userResponse.Should().NotBeNull();
        userResponse!.Name.Should().Be(fixture.DefaultUser.Name);
        userResponse.Role.Should().Be(fixture.DefaultUser.Role);
        userResponse.Username.Should().Be(fixture.DefaultUser.UserName);
        userResponse.Email.Should().Be(fixture.DefaultUser.Email);
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

    #region Delete

    #endregion
}