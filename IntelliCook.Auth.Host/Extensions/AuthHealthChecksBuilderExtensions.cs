using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Contexts;

namespace IntelliCook.Auth.Host.Extensions;

public static class AuthHealthChecksBuilderExtensions
{
    public static IHealthChecksBuilder AddAuthChecks(
        this IHealthChecksBuilder healthChecksBuilder,
        DatabaseOptions options
    )
    {
        return options.UseInMemory switch
        {
            true => healthChecksBuilder
                .AddDbContextCheck<AuthContext>(),
            _ => healthChecksBuilder
                .AddSqlServer(options.GetConnectionString(), name: "SqlServer")
                .AddDbContextCheck<AuthContext>()
        };
    }
}