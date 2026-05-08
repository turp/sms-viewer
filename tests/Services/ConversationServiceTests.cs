using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SmsViewer.Models;
using SmsViewer.Repositories;
using SmsViewer.Services;
using Xunit;

namespace SmsViewer.Tests.Services;

public class ConversationServiceTests
{
    private static IMessage Sms(string address, string contact, long date, string body, int type = 1) =>
        new SmsMessage(address, date, type, body, 1, -1, "Jan 1", contact);

    private static IMessage Mms(string address, string contact, long date, string body, params string[] addrs) =>
        new MmsMessage(address, date, body, 1, 1, "Jan 1", contact, []) { Addrs = addrs };

    private static Mock<ISmsRepository> RepoWith(params IMessage[] messages)
    {
        var mock = new Mock<ISmsRepository>();
        mock.Setup(r => r.GetMessagesAsync(It.IsAny<Stream>()))
            .Returns(ToAsync(messages));
        return mock;
    }

    private static async IAsyncEnumerable<IMessage> ToAsync(
        IEnumerable<IMessage> items,
        [EnumeratorCancellation] CancellationToken _ = default)
    {
        foreach (var item in items) { await Task.Yield(); yield return item; }
    }

    private static Stream AnyStream() => new MemoryStream(Encoding.UTF8.GetBytes("<smses/>"));

    // ── GetConversationSummariesAsync ─────────────────────────────────────

    [Fact]
    public async Task Summaries_EmptyStream_ReturnsEmpty()
    {
        var service = new ConversationService(RepoWith().Object);
        var result = await service.GetConversationSummariesAsync(AnyStream());
        Assert.Empty(result);
    }

    [Fact]
    public async Task Summaries_TwoMessagesFromSameContact_ProducesOneSummary()
    {
        var repo = RepoWith(
            Sms("111", "Alice", 1000, "Hello"),
            Sms("111", "Alice", 2000, "World"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationSummariesAsync(AnyStream());

        Assert.Single(result);
        Assert.Equal(2, result[0].MessageCount);
    }

    [Fact]
    public async Task Summaries_TwoContacts_ProducesTwoSummaries()
    {
        var repo = RepoWith(
            Sms("111", "Alice", 1000, "Hi"),
            Sms("222", "Bob", 2000, "Hey"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationSummariesAsync(AnyStream());

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Summaries_SortedByMostRecentMessageDescending()
    {
        var repo = RepoWith(
            Sms("111", "Alice", 1000, "old"),
            Sms("222", "Bob", 9000, "new"),
            Sms("111", "Alice", 5000, "middle"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationSummariesAsync(AnyStream());

        Assert.Equal("222", result[0].Address);
        Assert.Equal("111", result[1].Address);
    }

    [Fact]
    public async Task Summaries_LastMessagePreview_ReflectsMostRecentMessage()
    {
        var repo = RepoWith(
            Sms("111", "Alice", 1000, "earlier"),
            Sms("111", "Alice", 2000, "latest"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationSummariesAsync(AnyStream());

        Assert.Equal("latest", result[0].LastMessagePreview);
    }

    [Fact]
    public async Task Summaries_LongBody_PreviewTruncatedAt60Chars()
    {
        var longBody = new string('x', 80);
        var repo = RepoWith(Sms("111", "Alice", 1000, longBody));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationSummariesAsync(AnyStream());

        Assert.Equal(61, result[0].LastMessagePreview.Length);
        Assert.EndsWith("…", result[0].LastMessagePreview);
    }

    [Fact]
    public async Task Summaries_ContactNameNull_DisplayNameFallsBackToAddress()
    {
        var repo = RepoWith(Sms("555", "null", 1000, "hi"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationSummariesAsync(AnyStream());

        Assert.Equal("555", result[0].DisplayName);
    }

    [Fact]
    public async Task Summaries_NoEagerThreadMaterialization_SummaryContainsMessageCount()
    {
        var repo = RepoWith(
            Sms("111", "Alice", 1000, "a"),
            Sms("111", "Alice", 2000, "b"),
            Sms("111", "Alice", 3000, "c"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationSummariesAsync(AnyStream());

        Assert.Single(result);
        Assert.Equal(3, result[0].MessageCount);
    }

    [Fact]
    public async Task Summaries_RcsGroupWithUnknownContact_ResolvesParticipantNamesFromAddrs()
    {
        var repo = RepoWith(
            Sms("111", "Alice", 1000, "hi"),
            Mms("rcs@group", "(Unknown)", 2000, "group msg", "111", "222"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationSummariesAsync(AnyStream());

        var group = result.First(s => s.Address == "rcs@group");
        Assert.Contains("Alice", group.DisplayName);
        Assert.Contains("222", group.DisplayName);
    }

    // ── GetConversationMessagesAsync ──────────────────────────────────────

    [Fact]
    public async Task Messages_ReturnsOnlyMessagesForGivenAddress()
    {
        var repo = RepoWith(
            Sms("111", "Alice", 1000, "Alice msg"),
            Sms("222", "Bob", 2000, "Bob msg"),
            Sms("111", "Alice", 3000, "Alice again"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationMessagesAsync(AnyStream(), "111");

        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.Equal("111", m.Address));
    }

    [Fact]
    public async Task Messages_SortedChronologically()
    {
        var repo = RepoWith(
            Sms("111", "Alice", 3000, "third"),
            Sms("111", "Alice", 1000, "first"),
            Sms("111", "Alice", 2000, "second"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationMessagesAsync(AnyStream(), "111");

        Assert.Equal("first", result[0].Body);
        Assert.Equal("second", result[1].Body);
        Assert.Equal("third", result[2].Body);
    }

    [Fact]
    public async Task Messages_UnknownAddress_ReturnsEmpty()
    {
        var repo = RepoWith(Sms("111", "Alice", 1000, "hi"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationMessagesAsync(AnyStream(), "999");

        Assert.Empty(result);
    }
}
