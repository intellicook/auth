using IntelliCook.Auth.Client.E2ETests.Fixtures.Given;
using IntelliCook.Auth.Host.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntelliCook.Auth.Client.E2ETests.Fixtures;

public class ClientFixture : IDisposable
{
    public const string AdminName = "Admin";

    public const string AdminEmail = "Admin@Example.com";

    public const string AdminUsername = "admin";

    public const string AdminPassword = "Password123!";

    public WebApplicationFactory<Program> Factory { get; }

    public AuthClient<AuthOptionsFixture> Client { get; }

    public JsonSerializerOptions SerializerOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public ClientFixture()
    {
        Environment.SetEnvironmentVariable(
            $"{DatabaseOptions.SectionKey}:{nameof(DatabaseOptions.UseInMemory)}",
            "true"
        );
        Environment.SetEnvironmentVariable(
            $"{AdminOptions.SectionKey}:{nameof(AdminOptions.Name)}",
            AdminName
        );
        Environment.SetEnvironmentVariable(
            $"{AdminOptions.SectionKey}:{nameof(AdminOptions.Email)}",
            AdminEmail
        );
        Environment.SetEnvironmentVariable(
            $"{AdminOptions.SectionKey}:{nameof(AdminOptions.Username)}",
            AdminUsername
        );
        Environment.SetEnvironmentVariable(
            $"{AdminOptions.SectionKey}:{nameof(AdminOptions.Password)}",
            AdminPassword
        );

        Factory = new WebApplicationFactory<Program>();
        Client = new AuthClient<AuthOptionsFixture>
        {
            Client = Factory.CreateClient()
        };
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        GC.SuppressFinalize(this);
    }

    public AuthClient<AuthOptionsFixture> ClientWithWebHostBuilder(Action<IWebHostBuilder> configuration)
    {
        return new AuthClient<AuthOptionsFixture>
        {
            Client = Factory.WithWebHostBuilder(configuration).CreateClient()
        };
    }

    public async Task<T> With<T>(T resource, bool willCleanup = true) where T : GivenBase
    {
        resource.Create(this, willCleanup);
        await resource.Init();
        return resource;
    }
}

[CollectionDefinition(nameof(ClientFixture))]
public class ClientCollection : ICollectionFixture<ClientFixture>;