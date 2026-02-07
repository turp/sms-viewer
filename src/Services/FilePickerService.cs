using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Controls;
using System.Linq;

namespace SmsViewer.Services;

/// <summary>
/// implements IFilePickerService using Avalonia's StorageProvider.
/// </summary>
public class FilePickerService : IFilePickerService
{
    private readonly Window _target;

    public FilePickerService(Window target)
    {
        _target = target;
    }

    public async Task<string?> PickXmlFileAsync()
    {
        var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open SMS Backup XML",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("XML Files") { Patterns = new[] { "*.xml" } }
            }
        });

        return files.FirstOrDefault()?.Path.LocalPath;
    }
}
