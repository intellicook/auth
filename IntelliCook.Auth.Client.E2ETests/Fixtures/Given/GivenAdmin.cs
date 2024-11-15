using FluentAssertions;
using IntelliCook.Auth.Contract.Auth.Login;
using System.Net;
using System.Text.Json;

namespace IntelliCook.Auth.Client.E2ETests.Fixtures.Given;

public class GivenAdmin : GivenBase
{
    private bool SetAuthorizationHeader { get; }

    public GivenAdmin()
    {
    }

    public GivenAdmin(bool setAuthorizationHeader)
    {
        SetAuthorizationHeader = setAuthorizationHeader;
    }

    public override async Task Init()
    {
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
            Username = ClientFixture.AdminUsername,
            Password = ClientFixture.AdminPassword
        };

        var response = await Fixture.Client.PostAuthLoginAsync(request);

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        response.IsSuccessful.Should().BeTrue(
            $"Status {response.StatusCode}: {(response.HasError, response.HasValidationError) switch
            {
                (true, true) => JsonSerializer.Serialize(response.ValidationError, jsonOptions),
                (true, false) => JsonSerializer.Serialize(response.Error, jsonOptions),
                _ => "No error message"
            }}");

        return response.Value.AccessToken;
    }
}