using IntelliCook.Auth.Contract.Auth.Login;
using IntelliCook.Auth.Contract.Auth.Register;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntelliCook.Auth.Host.E2ETests.Fixtures.Given;

public class GivenUser : GivenBase
{
    public string Name { get; set; } = "Default Name";

    public string Username { get; set; } = "Default_Username";

    public string Email { get; set; } = "Default.Email@Email.com";

    public string Password { get; set; } = "Default Password 1234";

    public override async Task Init()
    {
        var request = new RegisterPostRequestModel
        {
            Name = Name,
            Username = Username,
            Email = Email,
            Password = Password
        };

        var response = await Fixture.Client.PostAsJsonAsync("/Auth/Register", request, Fixture.SerializerOptions);

        response.EnsureSuccessStatusCode();
    }

    public override async Task Cleanup()
    {
        var token = await GetToken();
        var request = new HttpRequestMessage(HttpMethod.Delete, "/User/Me");
        request.Headers.Add("Authorization", $"Bearer {token}");

        var response = await Fixture.Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> GetToken()
    {
        var request = new LoginPostRequestModel
        {
            Username = Username,
            Password = Password
        };

        var response = await Fixture.Client.PostAsJsonAsync("/Auth/Login", request, Fixture.SerializerOptions);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<LoginPostResponseModel>(content, Fixture.SerializerOptions)?.AccessToken;

        return token ?? throw new InvalidOperationException("Failed to get token.");
    }
}