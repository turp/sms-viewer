using System.Threading.Tasks;

namespace SmsViewer.Services;

/// <summary>
/// Defines a service for picking files from the local file system.
/// </summary>
public interface IFilePickerService
{
    /// <summary>
    /// Opens a file picker dialog and returns the selected XML file path.
    /// </summary>
    /// <returns>The absolute path to the selected file, or null if canceled.</returns>
    Task<string?> PickXmlFileAsync();
}
