using IntelliCook.Auth.Contract.Auth.Login;
using IntelliCook.Auth.Contract.Auth.Register;
using IntelliCook.Auth.Contract.User;
using IntelliCook.Auth.Host.E2ETests.Fixtures.Given;
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

    public ClientFixture()
    {
        Environment.SetEnvironmentVariable(
            $"{DatabaseOptions.SectionKey}:{nameof(DatabaseOptions.UseInMemory)}",
            "true"
        );

        Factory = new WebApplicationFactory<Program>();
        Client = Factory.CreateClient();
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        GC.SuppressFinalize(this);
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