using IntelliCook.Auth.Contract.Auth.Register;
using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntelliCook.Auth.Host.E2ETests.Fixtures;

public class ClientFixture : IDisposable
{
    public WebApplicationFactory<Program> Factory { get; }

    public HttpClient Client { get; }

    public JsonSerializerOptions SerializerOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public IntelliCookUser DefaultUser { get; } = new()
    {
        Name = "Default Name",
        Role = UserRoleModel.Admin,
        UserName = "Default_Username",
        Email = "Default.Email@Email.com",
        PasswordHash = "Default Password Hash",
    };

    public string DefaultUserPassword { get; } = "Default Password 1234";

    public ClientFixture()
    {
        Environment.SetEnvironmentVariable(
            $"{DatabaseOptions.SectionKey}:{nameof(DatabaseOptions.UseInMemory)}",
            "true"
        );

        Factory = new WebApplicationFactory<Program>();
        Client = Factory.CreateClient();

        // TODO: Add per test method solution after implementing a delete user endpoint
        var task = GivenUser();
        task.Wait();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Client.Dispose();
        Factory.Dispose();
    }

    public async Task GivenUser(
        string? name = null,
        string? username = null,
        string? email = null,
        string? password = null
    )
    {
        var request = new RegisterPostRequestModel
        {
            Name = name ?? DefaultUser.Name,
            Username = username ?? DefaultUser.UserName,
            Email = email ?? DefaultUser.Email,
            Password = password ?? DefaultUserPassword
        };

        var response = await Client.PostAsJsonAsync("/Auth/Register", request, SerializerOptions);

        response.EnsureSuccessStatusCode();
    }
}

[CollectionDefinition(nameof(ClientFixture))]
public class ClientCollection : ICollectionFixture<ClientFixture>;