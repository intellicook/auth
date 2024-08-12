using IntelliCook.Auth.Contract.Health;
using IntelliCook.Auth.Host.Extensions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;

namespace IntelliCook.Auth.Host.Controllers;

[Route("[controller]")]
[ApiController]
[AllowAnonymous]
public class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    /// <summary>
    /// Checks the health of Auth and its components.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthGetResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthGetResponseModel), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get()
    {
        var report = await healthCheckService.CheckHealthAsync();
        var result = new HealthGetResponseModel
        {
            Status = report.Status.ToHealthStatusModel(),
            Checks = report.Entries.Select(entry => new HealthCheckModel
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToHealthStatusModel()
            })
        };

        return result.Status switch
        {
            HealthStatusModel.Healthy => Ok(result),
            _ => StatusCode((int)HttpStatusCode.ServiceUnavailable, result)
        };
    }
}