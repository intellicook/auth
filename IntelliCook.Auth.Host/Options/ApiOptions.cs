namespace IntelliCook.Auth.Host.Options;

public class ApiOptions : IOptionsBase
{
    public static string SectionKey => "Api";

    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public int MajorVersion { get; init; }
    public int MinorVersion { get; init; }

    public string VersionString => $"v{MajorVersion}.{MinorVersion}";

    public void Validate()
    {
        if (Title is null)
        {
            throw new InvalidOperationException("Title is required");
        }

        if (Description is null)
        {
            throw new InvalidOperationException("Description is required");
        }

        if (MajorVersion < 0)
        {
            throw new InvalidOperationException("MajorVersion must be greater than or equal to 0");
        }

        if (MinorVersion < 0)
        {
            throw new InvalidOperationException("MinorVersion must be greater than or equal to 0");
        }

        if (MajorVersion == 0 && MinorVersion == 0)
        {
            throw new InvalidOperationException("MajorVersion or MinorVersion must be greater than 0");
        }
    }
}