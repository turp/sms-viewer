using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SmsViewer.Services;
using Xunit;

namespace SmsViewer.Tests.Services;

public class ExportServiceTests
{
    private static string TempXmlFile(string xml)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, xml, Encoding.UTF8);
        return path;
    }

    [Fact]
    public async Task ExportThreadsAsync_WritesOnlySelectedAddresses()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="3">
              <sms address="A" date="1000" type="1" body="from A" read="1" status="-1" readable_date="Jan 1" contact_name="Alice" />
              <sms address="B" date="2000" type="1" body="from B" read="1" status="-1" readable_date="Jan 1" contact_name="Bob" />
              <sms address="C" date="3000" type="1" body="from C" read="1" status="-1" readable_date="Jan 1" contact_name="Carol" />
            </smses>
            """;

        var source = TempXmlFile(xml);
        try
        {
            var service = new ExportService();
            using var dest = new MemoryStream();
            await service.ExportThreadsAsync(source, ["A", "C"], dest);

            dest.Position = 0;
            var doc = XDocument.Load(dest);
            var smsElements = doc.Descendants("sms");
            Assert.Contains(smsElements, e => e.Attribute("address")?.Value == "A");
            Assert.DoesNotContain(doc.Descendants("sms"), e => e.Attribute("address")?.Value == "B");
            Assert.Contains(smsElements, e => e.Attribute("address")?.Value == "C");
        }
        finally { File.Delete(source); }
    }

    [Fact]
    public async Task ExportThreadsAsync_WritesCorrectCount()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="100">
              <sms address="A" date="1000" type="1" body="msg 1" read="1" status="-1" readable_date="Jan 1" contact_name="Alice" />
              <sms address="A" date="2000" type="1" body="msg 2" read="1" status="-1" readable_date="Jan 1" contact_name="Alice" />
              <sms address="B" date="3000" type="1" body="msg 3" read="1" status="-1" readable_date="Jan 1" contact_name="Bob" />
            </smses>
            """;

        var source = TempXmlFile(xml);
        try
        {
            var service = new ExportService();
            using var dest = new MemoryStream();
            await service.ExportThreadsAsync(source, ["A"], dest);

            dest.Position = 0;
            var doc = XDocument.Load(dest);
            Assert.Equal("2", doc.Root?.Attribute("count")?.Value);
        }
        finally { File.Delete(source); }
    }

    [Fact]
    public async Task ExportThreadsAsync_PreservesMmsPartsAndAttributes()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="1">
              <mms address="A" date="1000" read="1" msg_box="1" readable_date="Jan 1" contact_name="Alice">
                <parts>
                  <part ct="text/plain" name="null" text="hello" />
                  <part ct="image/jpeg" name="photo.jpg" text="" data="abc123" />
                </parts>
              </mms>
            </smses>
            """;

        var source = TempXmlFile(xml);
        try
        {
            var service = new ExportService();
            using var dest = new MemoryStream();
            await service.ExportThreadsAsync(source, ["A"], dest);

            dest.Position = 0;
            var doc = XDocument.Load(dest);
            var parts = doc.Descendants("part");
            Assert.Equal(2, parts.Count());
            Assert.Equal("abc123", parts.Last().Attribute("data")?.Value);
        }
        finally { File.Delete(source); }
    }

    [Fact]
    public async Task ExportThreadsAsync_SourceFileUnchangedAfterExport()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="2">
              <sms address="A" date="1000" type="1" body="msg A" read="1" status="-1" readable_date="Jan 1" contact_name="Alice" />
              <sms address="B" date="2000" type="1" body="msg B" read="1" status="-1" readable_date="Jan 1" contact_name="Bob" />
            </smses>
            """;

        var source = TempXmlFile(xml);
        try
        {
            var originalContent = File.ReadAllText(source);
            var service = new ExportService();
            using var dest = new MemoryStream();
            await service.ExportThreadsAsync(source, ["A"], dest);

            Assert.Equal(originalContent, File.ReadAllText(source));
        }
        finally { File.Delete(source); }
    }

    [Fact]
    public async Task ExportThreadsAsync_MultipleThreads_AllMessagesIncluded()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="4">
              <sms address="A" date="1000" type="1" body="A1" read="1" status="-1" readable_date="Jan 1" contact_name="Alice" />
              <sms address="B" date="2000" type="1" body="B1" read="1" status="-1" readable_date="Jan 1" contact_name="Bob" />
              <sms address="A" date="3000" type="2" body="A2" read="1" status="-1" readable_date="Jan 1" contact_name="Alice" />
              <sms address="C" date="4000" type="1" body="C1" read="1" status="-1" readable_date="Jan 1" contact_name="Carol" />
            </smses>
            """;

        var source = TempXmlFile(xml);
        try
        {
            var service = new ExportService();
            using var dest = new MemoryStream();
            await service.ExportThreadsAsync(source, ["A", "B"], dest);

            dest.Position = 0;
            var doc = XDocument.Load(dest);
            Assert.Equal("3", doc.Root?.Attribute("count")?.Value);
            Assert.DoesNotContain(doc.Descendants("sms"), e => e.Attribute("address")?.Value == "C");
        }
        finally { File.Delete(source); }
    }
}
