using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Contexts;
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
}