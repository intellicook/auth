using FluentAssertions;
using IntelliCook.Auth.Client.E2ETests.Fixtures;
using IntelliCook.Auth.Contract.Auth.Login;
using IntelliCook.Auth.Contract.Auth.Register;
using IntelliCook.Auth.Contract.User;
using System.Net;

namespace IntelliCook.Auth.Client.E2ETests.Endpoints.Auth;

[Collection(nameof(ClientFixture))]
public class RegisterControllerTests(ClientFixture fixture)
{
    #region Post

    [Fact]
    public async void Post_Valid_ReturnsCreatedResult()
    {
        // Arrange
        var request = new RegisterPostRequestModel
        {
            Name = "Name",
            Username = "Username",
            Email = "Email@Email.com",
            Password = "Password1234"
        };

        // Act
        var result = await fixture.Client.PostAuthRegisterAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginRequest = new LoginPostRequestModel
        {
            Username = request.Username,
            Password = request.Password
        };
        var loginResult = await fixture.Client.PostAuthLoginAsync(loginRequest);
        loginResult.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResult.Value.AccessToken.Should().NotBeNullOrEmpty();

        fixture.Client.RequestHeaders.Add("Authorization", $"Bearer {loginResult.Value.AccessToken}");

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
        var deleteResult = await fixture.Client.DeleteUserMeAsync();
        deleteResult.StatusCode.Should().Be(HttpStatusCode.NoContent);
        fixture.Client.RequestHeaders.Remove("Authorization");
    }

    public static IEnumerable<object[]> Post_Invalid_ReturnsBadRequest_TestData()
    {
        yield return
        [
            new RegisterPostRequestModel
            {
                Name = "",
                Username = "Username",
                Email = "Email@Email.com",
                Password = "Password1234"
            },
            new[] { "Name" }
        ];
        yield return
        [
            new RegisterPostRequestModel
            {
                Name = "Name",
                Username = "",
                Email = "Email@Email.com",
                Password = "Password1234"
            },
            new[] { "Username" }
        ];
        yield return
        [
            new RegisterPostRequestModel
            {
                Name = "Name",
                Username = "Username",
                Email = "Email",
                Password = "Password1234"
            },
            new[] { "Email" }
        ];
        yield return
        [
            new RegisterPostRequestModel
            {
                Name = "Name",
                Username = "Username",
                Email = "Email@Email.com",
                Password = "Password"
            },
            new[] { "Password" }
        ];
        yield return
        [
            new RegisterPostRequestModel
            {
                Name = "",
                Username = "",
                Email = "",
                Password = ""
            },
            new[] { "Name", "Username", "Email", "Password" }
        ];
    }

    [Theory]
    [MemberData(nameof(Post_Invalid_ReturnsBadRequest_TestData))]
    public async void Post_Invalid_ReturnsBadRequest(RegisterPostRequestModel request, string[] expectedErrors)
    {
        // Act
        var result = await fixture.Client.PostAuthRegisterAsync(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        foreach (var property in expectedErrors)
        {
            result.ValidationError?.Errors.Should().ContainKey(property);
        }
    }

    #endregion
}