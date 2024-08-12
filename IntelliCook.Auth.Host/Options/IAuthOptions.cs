namespace IntelliCook.Auth.Host.Options;

public interface IAuthOptions
{
    public static abstract string SectionKey { get; }

    public void Validate()
    {
    }
}