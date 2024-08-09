using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntelliCook.Auth.Host.Controllers.Api;

[Route("Api/[controller]")]
[ApiController]
[AllowAnonymous]
public class TemporaryController : ControllerBase
{
    /// <summary>
    /// Get method for temporary testing purposes.
    /// </summary>
    [HttpGet]
    public ActionResult Get()
    {
        return Ok("Temporary Controller for testing purposes.");
    }
}