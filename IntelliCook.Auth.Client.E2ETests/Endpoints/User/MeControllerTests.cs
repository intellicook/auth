using FluentAssertions;
using IntelliCook.Auth.Client.E2ETests.Fixtures;
using IntelliCook.Auth.Client.E2ETests.Fixtures.Given;
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