using FluentAssertions;
using IntelliCook.Auth.Contract.Auth.Login;
using IntelliCook.Auth.Contract.Auth.Register;
using System.Net;
using System.Text.Json;

namespace IntelliCook.Auth.Client.E2ETests.Fixtures.Given;

public class GivenUser : GivenBase
{
    public string Name { get; set; } = "Default Name";

    public string Username { get; set; } = "Default_Username";

    public string Email { get; set; } = "Default.Email@Email.com";

    public string Password { get; set; } = "Default Password 1234";

    private bool SetAuthorizationHeader { get; }

    public GivenUser()
    {
    }

    public GivenUser(bool setAuthorizationHeader)
    {
        SetAuthorizationHeader = setAuthorizationHeader;
    }

    public override async Task Init()
    {
        var request = new RegisterPostRequestModel
        {
            Name = Name,
            Username = Username,
            Email = Email,
            Password = Password
        };

        var result = await Fixture.Client.PostAuthRegisterAsync(request);

        result.StatusCode.Should().Be(HttpStatusCode.Created);

        if (SetAuthorizationHeader)
        {
            Fixture.Client.RequestHeaders.Remove("Authorization");
            var token = await GetToken();
            Fixture.Client.RequestHeaders.Add("Authorization", $"Bearer {token}");
        }
    }

    public override async Task Cleanup()
    {
        Fixture.Client.RequestHeaders.Remove("Authorization");
        var token = await GetToken();
        Fixture.Client.RequestHeaders.Add("Authorization", $"Bearer {token}");

        var result = await Fixture.Client.DeleteUserMeAsync();

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Fixture.Client.RequestHeaders.Remove("Authorization");
    }

    public async Task<string> GetToken()
    {
        var request = new LoginPostRequestModel
        {
            Username = Username,
            Password = Password
        };

        var response = await Fixture.Client.PostAuthLoginAsync(request);

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        response.IsSuccessful.Should().BeTrue($"Status {response.StatusCode}: {(response.HasError, response.HasValidationError) switch
        {
            (true, true) => JsonSerializer.Serialize(response.ValidationError, jsonOptions),
            (true, false) => JsonSerializer.Serialize(response.Error, jsonOptions),
            _ => "No error message"
        }}");

        return response.Value.AccessToken;
    }
}