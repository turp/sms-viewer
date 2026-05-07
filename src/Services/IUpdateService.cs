using System.Threading.Tasks;

namespace SmsViewer.Services;

public interface IUpdateService
{
    string? AvailableVersion { get; }
    Task<bool> CheckForUpdateAsync();
    Task ApplyUpdateAndRestartAsync();
}
