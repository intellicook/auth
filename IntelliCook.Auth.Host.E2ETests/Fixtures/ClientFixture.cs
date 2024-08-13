using IntelliCook.Auth.Contract.Auth.Login;
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
    public class GivenResource<T>(T resource, Func<Task> cleanup) : IDisposable, IAsyncDisposable
    {
        public T Resource { get; } = resource;

        private Func<Task> Cleanup { get; } = cleanup;

        public void Dispose()
        {
            Cleanup().Wait();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await Cleanup();
            GC.SuppressFinalize(this);
        }
    }

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
        Role = UserRoleModel.None,
        UserName = "Default_Username",
        Email = "Default.Email@Email.com",
        PasswordHash = "Default Password Hash"
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
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<GivenResource<(string name, string username, string email, string password)>> GivenUser(
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

        return new GivenResource<(string name, string username, string email, string password)>(
            (request.Name, request.Username, request.Email, request.Password),
            async () =>
            {
                var token = await GetToken(request.Username, request.Password);
                var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/User/Me");
                deleteRequest.Headers.Add("Authorization", $"Bearer {token}");

                var deleteResponse = await Client.SendAsync(deleteRequest);
                deleteResponse.EnsureSuccessStatusCode();
            }
        );
    }

    /// <summary>
    ///     Requires `GivenUser` to be called first.
    /// </summary>
    public async Task<string> GetToken(string? username = null, string? password = null)
    {
        var request = new LoginPostRequestModel
        {
            Username = username ?? DefaultUser.UserName,
            Password = password ?? DefaultUserPassword
        };

        var response = await Client.PostAsJsonAsync("/Auth/Login", request, SerializerOptions);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<LoginPostResponseModel>(content, SerializerOptions)?.AccessToken;

        return token ?? throw new InvalidOperationException("Failed to get token.");
    }
}

[CollectionDefinition(nameof(ClientFixture))]
public class ClientCollection : ICollectionFixture<ClientFixture>;