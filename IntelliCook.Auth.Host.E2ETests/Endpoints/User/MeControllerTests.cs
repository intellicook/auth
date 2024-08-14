using FluentAssertions;
using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Host.E2ETests.Fixtures;
using IntelliCook.Auth.Host.E2ETests.Fixtures.Given;
using Microsoft.AspNetCore.Mvc;
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
        userResponse!.Name.Should().Be(user.Name);
        userResponse.Username.Should().Be(user.Username);
        userResponse.Email.Should().Be(user.Email);
        userResponse.Role.Should().Be(UserRoleModel.None);
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

    [Fact]
    public async void Delete_Success_ReturnsNoContentResult()
    {
        // Arrange
        await using var user = await fixture.With(new GivenUser(), willCleanup: false);
        var token = await user.GetToken();
        var request = new HttpRequestMessage(HttpMethod.Delete, Path);
        request.Headers.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await fixture.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();

        var getRequest = new HttpRequestMessage(HttpMethod.Get, Path);
        getRequest.Headers.Add("Authorization", $"Bearer {token}");

        var getResponse = await fixture.Client.SendAsync(getRequest);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async void Delete_TokenMissing_ReturnsUnauthorizedObjectResult()
    {
        // Act
        var response = await fixture.Client.DeleteAsync(Path);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}