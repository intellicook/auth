using IntelliCook.Auth.Host.Extensions;
using IntelliCook.Auth.Host.Options;
using IntelliCook.Auth.Infrastructure.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

namespace IntelliCook.Auth.Host;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;

        ApiOptions = Configuration.GetAuthOptions<ApiOptions>();
        DatabaseOptions = Configuration.GetAuthOptions<DatabaseOptions>();
        JwtOptions = Configuration.GetAuthOptions<JwtOptions>();
        AdminOptions = Configuration.GetAuthOptions<AdminOptions>();
    }

    private IConfiguration Configuration { get; }
    private ApiOptions ApiOptions { get; }
    private DatabaseOptions DatabaseOptions { get; }
    private JwtOptions JwtOptions { get; }
    private AdminOptions AdminOptions { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthOptions<DatabaseOptions>(Configuration);
        services.AddAuthOptions<ApiOptions>(Configuration);
        services.AddAuthOptions<JwtOptions>(Configuration);
        services.AddAuthOptions<AdminOptions>(Configuration);
        services.AddAuthContext(DatabaseOptions);
        services.AddHealthChecks()
            .AddAuthChecks(DatabaseOptions);
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddControllers(o => o.Filters.Add(new ProducesAttribute("application/json")))
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddAuthSwagger(ApiOptions);
        services.AddAuthIdentity();
        services.AddAuthJwtAuthentication(JwtOptions);
    }

    public void Configure(WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            // Ensure database is created
            var context = services.GetRequiredService<AuthContext>();
            context.Database.Migrate();

            // Seed admin user
            services.SeedAuthAdminUser();
        }

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint($"/swagger/{ApiOptions.VersionString}/swagger.json", ApiOptions.VersionString);
                o.EnablePersistAuthorization();
            });
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}