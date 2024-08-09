using FluentAssertions;
using IntelliCook.Auth.Host.E2ETests.Fixtures;
using IntelliCook.Auth.Host.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace IntelliCook.Auth.Host.E2ETests.QualityAssurance;

[Collection(nameof(ClientFixture))]
public class OpenApiTests(ClientFixture fixture)
{
    private readonly ApiOptions _apiOptions = fixture.Factory.Services.GetRequiredService<IOptions<ApiOptions>>().Value;

    [Fact]
    public void Endpoints_HaveSummary()
    {
        // Arrange
        var client = fixture.Client;

        // Act
        var document = GetOpenApiDocument(client);

        // Assert
        document.Should().NotBeNull();

        foreach (var (path, item) in document!.Paths)
        {
            item.Should().NotBeNull();

            foreach (var (method, operation) in item.Operations)
            {
                operation.Should().NotBeNull();
                operation!.Summary.Should().NotBeNullOrEmpty($"summary is required for {method} {path}");
            }
        }
    }

    private OpenApiDocument? GetOpenApiDocument(HttpClient client)
    {
        var url = $"/swagger/{_apiOptions.VersionString}/swagger.json";
        var response = client.GetAsync(url).Result;
        response.EnsureSuccessStatusCode();
        var content = response.Content.ReadAsStringAsync().Result;
        return new OpenApiStringReader().Read(content, out _);
    }
}