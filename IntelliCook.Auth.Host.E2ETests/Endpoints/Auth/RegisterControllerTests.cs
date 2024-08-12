using FluentAssertions;
using IntelliCook.Auth.Contract.Auth.Register;
using IntelliCook.Auth.Host.E2ETests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntelliCook.Auth.Host.E2ETests.Endpoints.Auth;

[Collection(nameof(ClientFixture))]
public class RegisterControllerTests(ClientFixture fixture)
{
    private const string Path = "/Auth/Register";
    private readonly HttpClient _client = fixture.Client;

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
        var response = await _client.PostAsJsonAsync(Path, request, fixture.SerializerOptions);

        // Assert
        response.EnsureSuccessStatusCode();
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
        var response = await _client.PostAsJsonAsync(Path, request, fixture.SerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        var errors = JsonSerializer.Deserialize<ValidationProblemDetails>(
            content,
            fixture.SerializerOptions
        );
        errors.Should().NotBeNull();

        foreach (var property in expectedErrors)
        {
            errors!.Errors.Should().ContainKey(property);
        }
    }

    #endregion
}