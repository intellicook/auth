using FluentAssertions;
using IntelliCook.Auth.Client.E2ETests.Fixtures;
using IntelliCook.Auth.Contract.Health;
using IntelliCook.Auth.Host.Extensions.Models;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using System.Net;

namespace IntelliCook.Auth.Client.E2ETests.Endpoints;

[Collection(nameof(ClientFixture))]
public class HealthControllerTests
{
    private readonly AuthClient<AuthOptionsFixture> _client;
    private readonly Mock<HealthCheckService> _healthCheckServiceMock = new();

    public HealthControllerTests(ClientFixture fixture)
    {
        _client = fixture.ClientWithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<HealthCheckService>(_ => _healthCheckServiceMock.Object);
            });
        });
    }

    #region Get

    public static IEnumerable<object[]> Get_Healthy_ReturnsOkObjectResult_TestData()
    {
        yield return
        [
            new List<(string name, HealthStatus healthStatus)>
            {
                ("Check1", HealthStatus.Healthy),
                ("Check2", HealthStatus.Healthy)
            },
            HealthStatusModel.Healthy
        ];
        yield return
        [
            new List<(string name, HealthStatus healthStatus)> { ("Check1", HealthStatus.Healthy) },
            HealthStatusModel.Healthy
        ];
        yield return
        [
            Enumerable.Empty<(string name, HealthStatus healthStatus)>(),
            HealthStatusModel.Healthy
        ];
    }

    [Theory]
    [MemberData(nameof(Get_Healthy_ReturnsOkObjectResult_TestData))]
    public async void Get_Healthy_ReturnsOkObjectResult(
        IReadOnlyCollection<(string name, HealthStatus healthStatus)> statuses,
        HealthStatusModel expectedStatus
    )
    {
        // Arrange
        var report = GetHealthReport(statuses);

        _healthCheckServiceMock
            .Setup(m => m.CheckHealthAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        // Act
        var result = await _client.GetHealthAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Value.Should().BeEquivalentTo(new HealthGetResponseModel
        {
            Status = expectedStatus,
            Checks = statuses.Select(s => new HealthCheckModel
            {
                Name = s.name,
                Status = s.healthStatus.ToHealthStatusModel()
            })
        });
    }

    public static IEnumerable<object[]> Get_UnhealthyOrDegraded_ReturnsServiceUnavailableObjectResult_TestData()
    {
        yield return
        [
            new List<(string name, HealthStatus healthStatus)>
            {
                ("Check1", HealthStatus.Unhealthy), ("Check2", HealthStatus.Healthy)
            },
            HealthStatusModel.Unhealthy
        ];
        yield return
        [
            new List<(string name, HealthStatus healthStatus)>
            {
                ("Check1", HealthStatus.Degraded), ("Check2", HealthStatus.Healthy)
            },
            HealthStatusModel.Degraded
        ];
        yield return
        [
            new List<(string name, HealthStatus healthStatus)>
            {
                ("Check1", HealthStatus.Degraded), ("Check2", HealthStatus.Unhealthy)
            },
            HealthStatusModel.Unhealthy
        ];
        yield return
        [
            new List<(string name, HealthStatus healthStatus)>
            {
                ("Check1", HealthStatus.Degraded), ("Check2", HealthStatus.Degraded)
            },
            HealthStatusModel.Degraded
        ];
        yield return
        [
            new List<(string name, HealthStatus healthStatus)>
            {
                ("Check1", HealthStatus.Unhealthy), ("Check2", HealthStatus.Unhealthy)
            },
            HealthStatusModel.Unhealthy
        ];
    }


    [Theory]
    [MemberData(nameof(Get_UnhealthyOrDegraded_ReturnsServiceUnavailableObjectResult_TestData))]
    public async void Get_UnhealthyOrDegraded_ReturnsServiceUnavailableObjectResult(
        IReadOnlyCollection<(string name, HealthStatus healthStatus)> statuses,
        HealthStatusModel expectedStatus
    )
    {
        // Arrange
        var report = GetHealthReport(statuses);

        _healthCheckServiceMock
            .Setup(m => m.CheckHealthAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        // Act
        var result = await _client.GetHealthAsync();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        result.Error.Should().BeEquivalentTo(new HealthGetResponseModel
        {
            Status = expectedStatus,
            Checks = statuses.Select(s => new HealthCheckModel
            {
                Name = s.name,
                Status = s.healthStatus.ToHealthStatusModel()
            })
        });
    }

    #endregion

    private static HealthReport GetHealthReport(IReadOnlyCollection<(string name, HealthStatus healthStatus)> statuses)
    {
        return new HealthReport(
            statuses.ToDictionary(
                s => s.name,
                s => new HealthReportEntry(
                    s.healthStatus,
                    null,
                    TimeSpan.FromMilliseconds(1),
                    null,
                    null
                )
            ),
            TimeSpan.FromMilliseconds(3)
        );
    }
}