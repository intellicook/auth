namespace IntelliCook.Auth.Host.E2ETests.Fixtures.Given;

public abstract class GivenBase : IDisposable, IAsyncDisposable
{
    private ClientFixture? _fixture;

    protected ClientFixture Fixture => _fixture ?? throw new InvalidOperationException("GivenBase class must be created with ClientFixture.Given.");

    public GivenBase()
    { }

    public void Create(ClientFixture fixture)
    {
        _fixture = fixture;
    }

    public abstract Task Init();

    public virtual Task Cleanup()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Cleanup().Wait();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await Cleanup();
        GC.SuppressFinalize(this);
    }
}