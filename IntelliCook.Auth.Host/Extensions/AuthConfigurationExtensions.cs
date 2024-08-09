using IntelliCook.Auth.Host.Options;

namespace IntelliCook.Auth.Host.Extensions;

public static class AuthConfigurationExtensions
{
    public static IConfigurationSection GetValidatedSection<TOptions>(this IConfiguration configuration)
        where TOptions : IOptionsBase
    {
        var section = configuration.GetSection(TOptions.SectionKey);

        // Checks for existence
        if (!section.Exists())
        {
            throw new InvalidOperationException(
                $"{typeof(TOptions).Name} options section '{TOptions.SectionKey}' not found");
        }

        // Validation
        var options = section.Get<TOptions>();
        if (options is null)
        {
            throw new InvalidOperationException(
                $"Failed to bind {typeof(TOptions).Name} options section '{TOptions.SectionKey}'");
        }

        options.Validate();

        return section;
    }

    public static TOptions GetAuthOptions<TOptions>(this IConfiguration configuration)
        where TOptions : IOptionsBase
    {
        return configuration.GetValidatedSection<TOptions>().Get<TOptions>()!;
    }
}