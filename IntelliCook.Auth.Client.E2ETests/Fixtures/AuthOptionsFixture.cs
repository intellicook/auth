namespace IntelliCook.Auth.Client.E2ETests.Fixtures;

public class AuthOptionsFixture : IAuthOptions
{
    public Uri BaseUrl { get; set; } = new("http://localhost");
}