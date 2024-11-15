using FluentAssertions;
using IntelliCook.Auth.Client.E2ETests.Fixtures;
using IntelliCook.Auth.Client.E2ETests.Fixtures.Given;
using IntelliCook.Auth.Contract.User;
using System.Net;

namespace IntelliCook.Auth.Client.E2ETests.Endpoints.Admin;

[Collection(nameof(ClientFixture))]
public class UsersController(ClientFixture fixture)
{
    #region Get

    [Fact]
    public async void Get_Success_ReturnsOkObjectResult()
    {
        // Arrange
        await using var user = await fixture.With(new GivenUser());
        await using var admin = await fixture.With(new GivenAdmin(true));

        // Act
        var result = await fixture.Client.GetAdminUsersAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Value.Should().BeEquivalentTo(new[]
        {
            new UserGetResponseModel
            {
                Name = user.Name,
                Username = user.Username,
                Email = user.Email,
                Role = UserRoleModel.User
            },
            new UserGetResponseModel
            {
                Name = ClientFixture.AdminName,
                Username = ClientFixture.AdminUsername,
                Email = ClientFixture.AdminEmail,
                Role = UserRoleModel.Admin
            }
        });
    }

    #endregion
}