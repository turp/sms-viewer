using System.Threading.Tasks;
using SmsViewer.Services;

namespace SmsViewer.Tests.Services;

public class FakeUpdateService : IUpdateService
{
    private readonly bool _hasUpdate;
    private readonly string? _version;

    public bool ApplyWasCalled { get; private set; }

    public FakeUpdateService(bool hasUpdate = false, string? version = null)
    {
        _hasUpdate = hasUpdate;
        _version = version;
    }

    public string? AvailableVersion => _hasUpdate ? _version : null;

    public Task<bool> CheckForUpdateAsync() => Task.FromResult(_hasUpdate);

    public Task ApplyUpdateAndRestartAsync()
    {
        ApplyWasCalled = true;
        return Task.CompletedTask;
    }
}
