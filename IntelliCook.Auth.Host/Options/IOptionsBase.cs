namespace IntelliCook.Auth.Host.Options;

public interface IOptionsBase
{
    public static abstract string SectionKey { get; }

    public void Validate()
    {
    }
}