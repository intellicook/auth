using IntelliCook.Auth.Client;
using IntelliCook.Auth.Host.E2ETests.Fixtures.Given;
using IntelliCook.Auth.Host.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntelliCook.Auth.Host.E2ETests.Fixtures;

public class ClientFixture : IDisposable
{
    public WebApplicationFactory<Program> Factory { get; }

    public AuthClient Client { get; }

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

        Factory = new WebApplicationFactory<Program>();
        Client = new AuthClient
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

    public AuthClient ClientWithWebHostBuilder(Action<IWebHostBuilder> configuration)
    {
        return new AuthClient
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