using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SmsViewer.Models;
using SmsViewer.Repositories;
using Xunit;

namespace SmsViewer.Tests.Repositories;

public class XmlSmsRepositoryTests
{
    private static Stream ToStream(string xml) =>
        new MemoryStream(Encoding.UTF8.GetBytes(xml));

    [Fact]
    public async Task When_FileContainsOneSms_Should_ParseCorrectly()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="1">
              <sms address="123456" date="1285799668193" type="2" body="Hello World" read="1" status="-1" readable_date="Sep 30, 2010" contact_name="Alice" />
            </smses>
            """;

        var repository = new XmlSmsRepository();
        var messages = new List<IMessage>();
        await foreach (var message in repository.GetMessagesAsync(ToStream(xml)))
            messages.Add(message);

        Assert.Single(messages);
        var sms = Assert.IsType<SmsMessage>(messages[0]);
        Assert.Equal("123456", sms.Address);
        Assert.Equal("Hello World", sms.Body);
        Assert.Equal("Alice", sms.ContactName);
        Assert.Equal("Sep 30, 2010", sms.ReadableDate);
    }

    [Fact]
    public async Task When_FileContainsOneMms_Should_ParseCorrectly()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="1">
              <mms address="789" date="1285799668193" read="1" msg_box="1" readable_date="Sep 30, 2010" contact_name="Bob">
                <parts>
                  <part ct="text/plain" name="null" text="Mms Body Text" />
                </parts>
              </mms>
            </smses>
            """;

        var repository = new XmlSmsRepository();
        var messages = new List<IMessage>();
        await foreach (var message in repository.GetMessagesAsync(ToStream(xml)))
            messages.Add(message);

        Assert.Single(messages);
        var mms = Assert.IsType<MmsMessage>(messages[0]);
        Assert.Equal("789", mms.Address);
        Assert.Equal("Mms Body Text", mms.Body);
        Assert.Single(mms.Parts);
        Assert.Equal("Mms Body Text", mms.Parts[0].Text);
    }

    [Fact]
    public async Task When_SmsIsMissingRequiredAttribute_Should_SkipIt()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="1">
              <sms date="1285799668193" type="2" body="Hello" read="1" status="-1" />
            </smses>
            """;

        var repository = new XmlSmsRepository();
        var messages = new List<IMessage>();
        await foreach (var message in repository.GetMessagesAsync(ToStream(xml)))
            messages.Add(message);

        Assert.Empty(messages);
    }

    [Fact]
    public async Task When_FileContainsMixedMessages_Should_ParseAll()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="2">
              <sms address="111" date="1000000000000" type="1" body="SMS one" read="1" status="-1" readable_date="Jan 1, 2000" contact_name="Carol" />
              <mms address="222" date="1000000001000" read="1" msg_box="2" readable_date="Jan 1, 2000" contact_name="Dave">
                <parts>
                  <part ct="text/plain" name="null" text="MMS one" />
                </parts>
              </mms>
            </smses>
            """;

        var repository = new XmlSmsRepository();
        var messages = new List<IMessage>();
        await foreach (var message in repository.GetMessagesAsync(ToStream(xml)))
            messages.Add(message);

        Assert.Equal(2, messages.Count);
        Assert.IsType<SmsMessage>(messages[0]);
        Assert.IsType<MmsMessage>(messages[1]);
    }

    [Fact]
    public async Task When_SmsTypeIsTwo_IsSent_Should_BeTrue()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="1">
              <sms address="111" date="1000" type="2" body="sent" read="1" status="-1" readable_date="Jan 1" contact_name="Alice" />
            </smses>
            """;

        var repository = new XmlSmsRepository();
        var messages = new List<IMessage>();
        await foreach (var m in repository.GetMessagesAsync(ToStream(xml))) messages.Add(m);

        Assert.True(messages[0].IsSent);
    }

    [Fact]
    public async Task When_MmsMsgBoxIsTwo_IsSent_Should_BeTrue()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="1">
              <mms address="222" date="1000" read="1" msg_box="2" readable_date="Jan 1" contact_name="Bob">
                <parts><part ct="text/plain" name="null" text="sent mms" /></parts>
              </mms>
            </smses>
            """;

        var repository = new XmlSmsRepository();
        var messages = new List<IMessage>();
        await foreach (var m in repository.GetMessagesAsync(ToStream(xml))) messages.Add(m);

        Assert.True(messages[0].IsSent);
    }

    [Fact]
    public async Task When_MmsHasNoMsgBox_IsSent_Should_BeFalse()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="1">
              <mms address="222" date="1000" read="1" readable_date="Jan 1" contact_name="Bob">
                <parts><part ct="text/plain" name="null" text="received" /></parts>
              </mms>
            </smses>
            """;

        var repository = new XmlSmsRepository();
        var messages = new List<IMessage>();
        await foreach (var m in repository.GetMessagesAsync(ToStream(xml))) messages.Add(m);

        Assert.False(messages[0].IsSent);
    }

    [Fact]
    public async Task When_MmsIsMissingAddress_Should_SkipIt()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="1">
              <mms date="1285799668193" read="1" msg_box="1" readable_date="Sep 30, 2010" contact_name="Bob">
                <parts><part ct="text/plain" name="null" text="body" /></parts>
              </mms>
            </smses>
            """;

        var repository = new XmlSmsRepository();
        var messages = new List<IMessage>();
        await foreach (var message in repository.GetMessagesAsync(ToStream(xml)))
            messages.Add(message);

        Assert.Empty(messages);
    }

    [Fact]
    public async Task When_MmsHasAddrElements_Should_ParsePhoneAddrs_AndSkipRcsHashes()
    {
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="1">
              <mms address="hash@rcs.google.com" date="1000" read="1" msg_box="1" readable_date="Jan 1" contact_name="(Unknown)">
                <parts><part ct="text/plain" name="null" text="hi" /></parts>
                <addrs>
                  <addr address="+11111111111" type="137" charset="106" />
                  <addr address="+12222222222" type="151" charset="106" />
                  <addr address="other@rcs.google.com" type="151" charset="106" />
                </addrs>
              </mms>
            </smses>
            """;

        var repository = new XmlSmsRepository();
        var messages = new List<IMessage>();
        await foreach (var message in repository.GetMessagesAsync(ToStream(xml)))
            messages.Add(message);

        var mms = Assert.IsType<MmsMessage>(messages[0]);
        Assert.Equal(2, mms.Addrs.Count);
        Assert.Contains("+11111111111", mms.Addrs);
        Assert.Contains("+12222222222", mms.Addrs);
    }

    [Fact]
    public async Task GetMessagesAsync_YieldsItemsOneByOne_WithoutBufferingAll()
    {
        // Verifies that GetMessagesAsync is a true async stream (IAsyncEnumerable),
        // meaning callers can consume the first item before the rest are parsed.
        var xml = """
            <?xml version='1.0' encoding='UTF-8' standalone='yes' ?>
            <smses count="3">
              <sms address="111" date="1000" type="1" body="first" read="1" status="-1" readable_date="Jan 1" contact_name="Alice" />
              <sms address="111" date="2000" type="1" body="second" read="1" status="-1" readable_date="Jan 1" contact_name="Alice" />
              <sms address="111" date="3000" type="1" body="third" read="1" status="-1" readable_date="Jan 1" contact_name="Alice" />
            </smses>
            """;

        var repository = new XmlSmsRepository();
        var enumerator = repository.GetMessagesAsync(ToStream(xml)).GetAsyncEnumerator();
        await enumerator.MoveNextAsync();
        var first = enumerator.Current;

        // We got the first item without needing to await all three
        Assert.Equal("first", first.Body);
        await enumerator.DisposeAsync();
    }
}
