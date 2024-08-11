using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Contexts;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IntelliCook.Auth.Host.Extensions;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddAuthOptions<TOptions>(
        this IServiceCollection serviceCollection,
        IConfiguration configuration
    ) where
        TOptions : class, IOptionsBase
    {
        return serviceCollection.Configure<TOptions>(configuration.GetValidatedSection<TOptions>());
    }

    public static IServiceCollection AddAuthContext(
        this IServiceCollection serviceCollection,
        DatabaseOptions options
    )
    {
        return serviceCollection.AddDbContext<AuthContext>(options.UseInMemory switch
        {
            true => o => o.UseInMemoryDatabase(options.Name),
            _ => o => o.UseSqlServer(options.ConnectionString)
        });
    }

    public static IServiceCollection AddAuthIdentity(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddAuthorization();
        serviceCollection.AddIdentityApiEndpoints<IntelliCookUser>(o =>
                {
                    o.Password.RequireDigit = true;
                    o.Password.RequireLowercase = true;
                    o.Password.RequireUppercase = true;
                    o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequiredLength = 8;

                    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    o.Lockout.MaxFailedAccessAttempts = 10;
                    o.Lockout.AllowedForNewUsers = true;

                    o.User.AllowedUserNameCharacters =
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                    o.User.RequireUniqueEmail = true;
                })
            .AddEntityFrameworkStores<AuthContext>();

        return serviceCollection;
    }
}