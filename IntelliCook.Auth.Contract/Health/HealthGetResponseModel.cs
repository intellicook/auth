using System.ComponentModel.DataAnnotations;

namespace IntelliCook.Auth.Contract.Health;

public class HealthGetResponseModel
{
    [Required]
    public required HealthStatusModel Status { get; set; }

    [Required]
    public required IEnumerable<HealthCheckModel> Checks { get; set; }
}