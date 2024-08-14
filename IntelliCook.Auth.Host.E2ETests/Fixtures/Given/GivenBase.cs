namespace IntelliCook.Auth.Host.E2ETests.Fixtures.Given;

public abstract class GivenBase : IDisposable, IAsyncDisposable
{
    private ClientFixture? _fixture;

    protected ClientFixture Fixture => _fixture ?? throw new InvalidOperationException("GivenBase class must be created with ClientFixture.Given.");

    private bool WillCleanup { get; set; } = true;

    public void Create(ClientFixture fixture, bool willCleanup = true)
    {
        _fixture = fixture;
        WillCleanup = willCleanup;
    }

    public abstract Task Init();

    protected virtual Task Cleanup()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!WillCleanup)
        {
            return;
        }

        Cleanup().Wait();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (!WillCleanup)
        {
            return;
        }

        await Cleanup();
        GC.SuppressFinalize(this);
    }
}