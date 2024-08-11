using IntelliCook.Auth.Host.Options;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntelliCook.Auth.Host.E2ETests.Fixtures;

public class ClientFixture : IDisposable
{
    public ClientFixture()
    {
        Environment.SetEnvironmentVariable(
            $"{DatabaseOptions.SectionKey}:{nameof(DatabaseOptions.UseInMemory)}",
            "true"
        );

        Factory = new WebApplicationFactory<Program>();
        Client = Factory.CreateClient();
    }

    public WebApplicationFactory<Program> Factory { get; }
    public HttpClient Client { get; }

    public JsonSerializerOptions SerializerOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Client.Dispose();
        Factory.Dispose();
    }
}

[CollectionDefinition(nameof(ClientFixture))]
public class ClientCollection : ICollectionFixture<ClientFixture>;