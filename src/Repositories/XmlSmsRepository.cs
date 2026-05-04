using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using SmsViewer.Models;

namespace SmsViewer.Repositories;

/// <summary>
/// Implements ISmsRepository using XmlReader for streaming large XML backup files.
/// </summary>
public class XmlSmsRepository : ISmsRepository
{
    public async IAsyncEnumerable<IMessage> GetMessagesAsync(Stream xmlStream)
    {
        var settings = new XmlReaderSettings { Async = true };
        using var reader = XmlReader.Create(xmlStream, settings);

        while (await reader.ReadAsync())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "sms")
                {
                    var message = ParseSms(reader);
                    if (message != null) yield return message;
                }
                else if (reader.Name == "mms")
                {
                    var message = await ParseMmsAsync(reader);
                    if (message != null) yield return message;
                }
            }
        }
    }

    private SmsMessage? ParseSms(XmlReader reader)
    {
        string? address = reader.GetAttribute("address");
        string? body = reader.GetAttribute("body");
        string? dateStr = reader.GetAttribute("date");

        if (address == null || body == null || dateStr == null)
            return null;

        return new SmsMessage(
            address,
            long.TryParse(dateStr, out var d) ? d : 0,
            int.TryParse(reader.GetAttribute("type"), out var t) ? t : 0,
            body,
            int.TryParse(reader.GetAttribute("read"), out var r) ? r : 0,
            int.TryParse(reader.GetAttribute("status"), out var s) ? s : 0,
            reader.GetAttribute("readable_date") ?? string.Empty,
            reader.GetAttribute("contact_name") ?? string.Empty
        );
    }

    private async Task<MmsMessage?> ParseMmsAsync(XmlReader reader)
    {
        string? address = reader.GetAttribute("address");
        string? dateStr = reader.GetAttribute("date");

        if (address == null || dateStr == null)
            return null;

        long date = long.TryParse(dateStr, out var d) ? d : 0;
        int read = int.TryParse(reader.GetAttribute("read"), out var r) ? r : 0;
        string readableDate = reader.GetAttribute("readable_date") ?? string.Empty;
        string contactName = reader.GetAttribute("contact_name") ?? string.Empty;

        var parts = new List<MmsPart>();

        if (!reader.IsEmptyElement)
        {
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "mms")
                    break;

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "part")
                {
                    parts.Add(new MmsPart(
                        reader.GetAttribute("ct") ?? string.Empty,
                        reader.GetAttribute("name") ?? string.Empty,
                        reader.GetAttribute("text") ?? string.Empty,
                        reader.GetAttribute("data") ?? string.Empty
                    ));
                }
            }
        }

        string body = parts.FirstOrDefault(p => p.ContentType == "text/plain")?.Text ?? string.Empty;
        return new MmsMessage(address, date, body, read, readableDate, contactName, parts);
    }
}
