using IntelliCook.Auth.Host.Extensions;
using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

namespace IntelliCook.Auth.Host;

public class Startup
{
    private IConfiguration Configuration { get; }
    private ApiOptions ApiOptions { get; }
    private DatabaseOptions DatabaseOptions { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;

        ApiOptions = Configuration.GetAuthOptions<ApiOptions>();
        DatabaseOptions = Configuration.GetAuthOptions<DatabaseOptions>();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthOptions<DatabaseOptions>(Configuration);
        services.AddAuthOptions<ApiOptions>(Configuration);
        services.AddAuthContext(DatabaseOptions);
        services.AddHealthChecks()
            .AddAuthChecks(DatabaseOptions);
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddControllers(o => o.Filters.Add(new ProducesAttribute("application/json")))
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(o =>
        {
            o.SwaggerDoc(ApiOptions.VersionString, new OpenApiInfo
            {
                Version = ApiOptions.VersionString,
                Title = ApiOptions.Title,
                Description = ApiOptions.Description
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        services.AddAuthIdentity();
    }

    public void Configure(WebApplication app)
    {
        // Ensure database is created
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<AuthContext>();
            context.Database.EnsureCreated();
        }

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint($"/swagger/{ApiOptions.VersionString}/swagger.json", ApiOptions.VersionString);
            });
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapGroup("Identity").MapIdentityApi<IdentityUser>().WithTags(["Identity"]);
        app.MapControllers();
    }
}