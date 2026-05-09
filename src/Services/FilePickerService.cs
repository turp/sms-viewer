using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace SmsViewer.Services;

/// <summary>
/// Implements IFilePickerService using Avalonia's IStorageProvider.
/// </summary>
public class FilePickerService : IFilePickerService
{
    private readonly IStorageProvider _storageProvider;

    public FilePickerService(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public async Task<string?> PickXmlFileAsync()
    {
        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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

    public async Task<string?> PickSaveXmlFileAsync(string suggestedName)
    {
        var file = await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save SMS Export",
            SuggestedFileName = suggestedName,
            DefaultExtension = "xml",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("XML Files") { Patterns = new[] { "*.xml" } }
            }
        });

        return file?.Path.LocalPath;
    }
}
