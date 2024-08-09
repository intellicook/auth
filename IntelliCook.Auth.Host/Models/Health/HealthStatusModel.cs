using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IntelliCook.Auth.Host.Models.Health;

public enum HealthStatusModel
{
    Healthy,
    Degraded,
    Unhealthy
}

public static class HealthStatusModelExtensions
{
    public static HealthStatusModel ToHealthStatusModel(this HealthStatus healthStatus)
    {
        return healthStatus switch
        {
            HealthStatus.Healthy => HealthStatusModel.Healthy,
            HealthStatus.Degraded => HealthStatusModel.Degraded,
            HealthStatus.Unhealthy => HealthStatusModel.Unhealthy,
            _ => throw new ArgumentOutOfRangeException(nameof(healthStatus), healthStatus, null)
        };
    }

    public static HealthStatus ToHealthStatus(this HealthStatusModel healthStatusModel)
    {
        return healthStatusModel switch
        {
            HealthStatusModel.Healthy => HealthStatus.Healthy,
            HealthStatusModel.Degraded => HealthStatus.Degraded,
            HealthStatusModel.Unhealthy => HealthStatus.Unhealthy,
            _ => throw new ArgumentOutOfRangeException(nameof(healthStatusModel), healthStatusModel, null)
        };
    }
}