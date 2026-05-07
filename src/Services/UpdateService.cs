using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace SmsViewer.Services;

public class UpdateService : IUpdateService
{
    // TODO: replace with actual GitHub repo URL before first release
    private const string RepoUrl = "https://github.com/turp/sms-viewer";

    private readonly UpdateManager _manager;
    private UpdateInfo? _pendingUpdate;

    public string? AvailableVersion => _pendingUpdate?.TargetFullRelease.Version?.ToString();

    public UpdateService()
    {
        _manager = new UpdateManager(new GithubSource(RepoUrl, null, false));
    }

    public async Task<bool> CheckForUpdateAsync()
    {
        try
        {
            _pendingUpdate = await _manager.CheckForUpdatesAsync();
            return _pendingUpdate != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task ApplyUpdateAndRestartAsync()
    {
        if (_pendingUpdate == null) return;
        await _manager.DownloadUpdatesAsync(_pendingUpdate);
        _manager.ApplyUpdatesAndRestart(_pendingUpdate);
    }
}
