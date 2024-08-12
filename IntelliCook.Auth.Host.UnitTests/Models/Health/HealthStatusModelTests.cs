using FluentAssertions;
using IntelliCook.Auth.Contract.Health;
using IntelliCook.Auth.Host.Extensions.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IntelliCook.Auth.Host.UnitTests.Models.Health;

public class HealthStatusModelTests
{
    #region ToHealthStatusModel

    [Theory]
    [InlineData(HealthStatus.Healthy, HealthStatusModel.Healthy)]
    [InlineData(HealthStatus.Degraded, HealthStatusModel.Degraded)]
    [InlineData(HealthStatus.Unhealthy, HealthStatusModel.Unhealthy)]
    public void ToHealthStatusModel_ValidHealthStatus_ReturnsExpectedHealthStatusModel(
        HealthStatus healthStatus,
        HealthStatusModel expectedHealthStatusModel
    )
    {
        // Act
        var result = healthStatus.ToHealthStatusModel();

        // Assert
        result.Should().Be(expectedHealthStatusModel);
    }

    [Fact]
    public void ToHealthStatusModel_InvalidHealthStatus_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const HealthStatus healthStatus = (HealthStatus)int.MaxValue;

        // Act
        var act = () => healthStatus.ToHealthStatusModel();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region ToHealthStatus

    [Theory]
    [InlineData(HealthStatusModel.Healthy, HealthStatus.Healthy)]
    [InlineData(HealthStatusModel.Degraded, HealthStatus.Degraded)]
    [InlineData(HealthStatusModel.Unhealthy, HealthStatus.Unhealthy)]
    public void ToHealthStatus_ValidHealthStatusModel_ReturnsExpectedHealthStatus(
        HealthStatusModel healthStatusModel,
        HealthStatus expectedHealthStatus
    )
    {
        // Act
        var result = healthStatusModel.ToHealthStatus();

        // Assert
        result.Should().Be(expectedHealthStatus);
    }

    [Fact]
    public void ToHealthStatus_InvalidHealthStatusModel_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const HealthStatusModel healthStatusModel = (HealthStatusModel)int.MaxValue;

        // Act
        var act = () => healthStatusModel.ToHealthStatus();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion
}