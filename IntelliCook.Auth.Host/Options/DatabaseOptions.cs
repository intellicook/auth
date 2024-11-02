namespace IntelliCook.Auth.Host.Options;

public class DatabaseOptions : IAuthOptions
{
    public static string SectionKey => "Database";

    public string Name { get; init; } = null!;

    public string? Host { get; init; }

    public string? Password { get; init; }

    public bool UseInMemory { get; init; }

    public string GetConnectionString()
    {
        if (UseInMemory)
        {
            throw new InvalidOperationException("ConnectionString is not available when UseInMemory is true");
        }

        return $"Server={Host},1433;Database={Name};User=sa;Password={Password};TrustServerCertificate=True";
    }

    public void Validate()
    {
        if (Name is null)
        {
            throw new InvalidOperationException("Name is required");
        }

        if (!UseInMemory && (Host is null || Password is null))
        {
            throw new InvalidOperationException("Host and Password are required when UseInMemory is false");
        }
    }
}