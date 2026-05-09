using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SmsViewer.Services;

public interface IExportService
{
    Task ExportThreadsAsync(string sourceFilePath, IEnumerable<string> addresses, Stream destination);
}
