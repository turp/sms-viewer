using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SmsViewer.Services;

public class ExportService : IExportService
{
    public async Task ExportThreadsAsync(string sourceFilePath, IEnumerable<string> addresses, Stream destination)
    {
        var addressSet = new HashSet<string>(addresses);
        var readerSettings = new XmlReaderSettings { Async = true };

        // Pass 1: count matching top-level elements (needed for <smses count="N">)
        int count = 0;
        {
            await using var s = File.OpenRead(sourceFilePath);
            using var r = XmlReader.Create(s, readerSettings);
            while (await r.ReadAsync())
            {
                if (r.NodeType == XmlNodeType.Element && r.Depth == 1 &&
                    (r.Name == "sms" || r.Name == "mms") &&
                    addressSet.Contains(r.GetAttribute("address") ?? ""))
                    count++;
            }
        }

        // Pass 2: write matching elements verbatim
        var writerSettings = new XmlWriterSettings
        {
            Async = true,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
            IndentChars = "  "
        };

        await using var writer = XmlWriter.Create(destination, writerSettings);
        await writer.WriteStartDocumentAsync();
        writer.WriteStartElement("smses");
        writer.WriteAttributeString("count", count.ToString(CultureInfo.InvariantCulture));

        {
            await using var s = File.OpenRead(sourceFilePath);
            using var r = XmlReader.Create(s, readerSettings);

            // needsRead tracks whether the reader must be advanced before checking the current node.
            // WriteNode advances the reader past the element it copies, so we skip ReadAsync after it.
            bool needsRead = true;
            while (true)
            {
                if (needsRead && !await r.ReadAsync()) break;
                needsRead = true;

                if (r.NodeType == XmlNodeType.Element && r.Depth == 1 &&
                    (r.Name == "sms" || r.Name == "mms") &&
                    addressSet.Contains(r.GetAttribute("address") ?? ""))
                {
                    writer.WriteNode(r, false);
                    needsRead = false;
                    if (r.EOF) break;
                }
            }
        }

        writer.WriteEndElement();
        await writer.FlushAsync();
    }
}
