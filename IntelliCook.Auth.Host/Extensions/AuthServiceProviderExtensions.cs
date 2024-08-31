using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Contexts;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IntelliCook.Auth.Host.Extensions;

public static class AuthServiceProviderExtensions
{
    public static void SeedAuthAdminUser(this IServiceProvider services)
    {
        var options = services.GetService<IOptions<AdminOptions>>()?.Value ??
                      throw new InvalidOperationException("AdminOptions not found");
        var userManager = services.GetService<UserManager<IntelliCookUser>>() ??
                         throw new InvalidOperationException("UserManager not found");
        var logger = services.GetService<ILogger<ServiceProvider>>() ??
                     throw new InvalidOperationException("ILogger not found");

        var adminUser = new IntelliCookUser
        {
            Name = options.Name,
            Role = options.Role,
            UserName = options.Username,
            Email = options.Email
        };

        if (userManager.FindByNameAsync(adminUser.UserName).Result != null)
        {
            logger.LogInformation("Admin user already exists");
            return;
        }

        var result = userManager.CreateAsync(adminUser, options.Password).Result;

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }

        logger.LogInformation("Admin user created");
    }
}