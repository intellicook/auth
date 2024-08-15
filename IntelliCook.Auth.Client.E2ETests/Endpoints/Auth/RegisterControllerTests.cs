using FluentAssertions;
using IntelliCook.Auth.Client.E2ETests.Fixtures;
using IntelliCook.Auth.Contract.Auth.Register;
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
        var result = await fixture.Client.PostAuthRegister(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);

        // TODO: Add more assertions
        // TODO: Delete user
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
        var result = await fixture.Client.PostAuthRegister(request);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        foreach (var property in expectedErrors)
        {
            result.ValidationError?.Errors.Should().ContainKey(property);
        }
    }

    #endregion
}