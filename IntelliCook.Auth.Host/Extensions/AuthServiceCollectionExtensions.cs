using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Contexts;
using IntelliCook.Auth.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

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

    public static IServiceCollection AddAuthSwagger(
        this IServiceCollection serviceCollection,
        ApiOptions apiOptions
    )
    {
        return serviceCollection
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(o =>
            {
                o.SwaggerDoc(apiOptions.VersionString, new OpenApiInfo
                {
                    Version = apiOptions.VersionString,
                    Title = apiOptions.Title,
                    Description = apiOptions.Description
                });

                o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Bearer",
                    BearerFormat = "JWT",
                    Scheme = "bearer",
                    Description = "Specify the authorization token.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http
                });

                o.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });
    }

    public static IServiceCollection AddAuthJwtAuthentication(
        this IServiceCollection serviceCollection,
        JwtOptions jwtOptions
    )
    {
        serviceCollection.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "https://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                    RoleClaimType = "https://schemas.microsoft.com/ws/2008/06/identity/claims/role",

                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),

                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateLifetime = false
                };
            });

        return serviceCollection;
    }

    public static IServiceCollection AddAuthIdentity(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddAuthorization();
        serviceCollection.AddIdentity<IntelliCookUser, IdentityRole>(o =>
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
            .AddEntityFrameworkStores<AuthContext>()
            .AddDefaultTokenProviders();

        return serviceCollection;
    }
}