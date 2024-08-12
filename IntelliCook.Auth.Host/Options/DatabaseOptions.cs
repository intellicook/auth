namespace IntelliCook.Auth.Host.Options;

public class DatabaseOptions : IAuthOptions
{
    public static string SectionKey => "Database";

    public string Name { get; init; } = null!;

    public string? ConnectionString { get; init; }

    public bool UseInMemory { get; init; }

    public void Validate()
    {
        if (Name is null)
        {
            throw new InvalidOperationException("Name is required");
        }

        if (!UseInMemory && ConnectionString is null)
        {
            throw new InvalidOperationException("ConnectionString is required when UseInMemory is false");
        }
    }
}