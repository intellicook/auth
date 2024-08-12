namespace IntelliCook.Auth.Host.Options;

public class JwtOptions : IAuthOptions
{
    public static string SectionKey => "Jwt";

    public string Secret { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
}