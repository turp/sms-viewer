using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SmsViewer.Models;
using SmsViewer.Repositories;
using Xunit;

namespace SmsViewer.Tests.Repositories;

public class XmlSmsRepositoryTests
{
    [Fact]
    public async Task When_FileContainsOneSms_Should_ParseCorrectly()
    {
        // Arrange
        var xml = @"<?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
<smses count=""1"">
  <sms address=""123456"" date=""1285799668193"" type=""2"" body=""Hello World"" read=""1"" status=""-1"" readable_date=""Sep 30, 2010"" contact_name=""Alice"" />
</smses>";
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        await File.WriteAllTextAsync(filePath, xml);

        try
        {
            var repository = new XmlSmsRepository();

            // Act
            var messages = new List<IMessage>();
            await foreach (var message in repository.GetMessagesAsync(filePath))
            {
                messages.Add(message);
            }

            // Assert
            Assert.Single(messages);
            var sms = Assert.IsType<SmsMessage>(messages[0]);
            Assert.Equal("123456", sms.Address);
            Assert.Equal("Hello World", sms.Body);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public async Task When_FileContainsOneMms_Should_ParseCorrectly()
    {
        // Arrange
        var xml = @"<?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
<smses count=""1"">
  <mms address=""789"" date=""1285799668193"" read=""1"" readable_date=""Sep 30, 2010"" contact_name=""Bob"">
    <parts>
      <part ct=""text/plain"" name=""null"" text=""Mms Body Text"" />
    </parts>
  </mms>
</smses>";
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        await File.WriteAllTextAsync(filePath, xml);

        try
        {
            var repository = new XmlSmsRepository();

            // Act
            var messages = new List<IMessage>();
            await foreach (var message in repository.GetMessagesAsync(filePath))
            {
                messages.Add(message);
            }

            // Assert
            Assert.Single(messages);
            var mms = Assert.IsType<MmsMessage>(messages[0]);
            Assert.Equal("789", mms.Address);
            Assert.Single(mms.Parts);
            Assert.Equal("Mms Body Text", mms.Parts[0].Text);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public async Task When_SmsIsMissingRequiredAttribute_Should_SkipIt()
    {
        // Arrange
        var xml = @"<?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
<smses count=""1"">
  <sms date=""1285799668193"" type=""2"" body=""Hello"" read=""1"" status=""-1"" />
</smses>";
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        await File.WriteAllTextAsync(filePath, xml);

        try
        {
            var repository = new XmlSmsRepository();

            // Act
            var messages = new List<IMessage>();
            await foreach (var message in repository.GetMessagesAsync(filePath))
            {
                messages.Add(message);
            }

            // Assert
            Assert.Empty(messages);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }
}
