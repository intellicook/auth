using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IntelliCook.Auth.Client;

public static class AuthService
{
    public static void AddAuthClient<TAuthOptions>(this IServiceCollection services, TAuthOptions options)
        where TAuthOptions : class, IAuthOptions
    {
        if (services.All(x => x.ServiceType != typeof(IHttpContextAccessor)))
        {
            throw new InvalidOperationException("IHttpContextAccessor is required for AuthClient");
        }

        services
            .AddOptions<TAuthOptions>()
            .Configure(o =>
            {
                o.BaseUrl = options.BaseUrl;
            });
        services.AddScoped<IAuthClient, AuthClient<TAuthOptions>>();
    }
}