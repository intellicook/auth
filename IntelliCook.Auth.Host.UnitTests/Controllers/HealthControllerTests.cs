using FluentAssertions;
using IntelliCook.Auth.Host.Controllers;
using IntelliCook.Auth.Host.Models.Health;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace IntelliCook.Auth.Host.UnitTests.Controllers;

public class HealthControllerTests
{
    private readonly HealthController _healthController;
    private readonly Mock<HealthCheckService> _healthCheckServiceMock = new();

    public HealthControllerTests()
    {
        _healthController = new HealthController(_healthCheckServiceMock.Object);
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
            .Setup(m => m.CheckHealthAsync(null, new CancellationToken()))
            .ReturnsAsync(report);

        // Act
        var result = await _healthController.Get();

        // Assert
        var response = result.Should().BeOfType<OkObjectResult>().Which
            .Value.Should().BeOfType<HealthGetResponseModel>().Subject;

        response.Status.Should().Be(expectedStatus);
        response.Checks.Should().BeEquivalentTo(statuses.Select(s => new HealthCheckModel
        {
            Name = s.name,
            Status = s.healthStatus.ToHealthStatusModel()
        }));
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
            .Setup(m => m.CheckHealthAsync(null, new CancellationToken()))
            .ReturnsAsync(report);

        // Act
        var result = await _healthController.Get();

        // Assert
        var response = result.Should().BeOfType<ObjectResult>().Which
            .Value.Should().BeOfType<HealthGetResponseModel>().Subject;

        response.Status.Should().Be(expectedStatus);
        response.Checks.Should().BeEquivalentTo(statuses.Select(s => new HealthCheckModel
        {
            Name = s.name,
            Status = s.healthStatus.ToHealthStatusModel()
        }));
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