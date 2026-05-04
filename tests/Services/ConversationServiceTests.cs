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

    [Fact]
    public async Task When_EmptyStream_Should_ReturnNoConversations()
    {
        var service = new ConversationService(RepoWith().Object);
        var result = await service.GetConversationsAsync(AnyStream());
        Assert.Empty(result);
    }

    [Fact]
    public async Task When_MessagesFromSameContact_Should_GroupIntoOneConversation()
    {
        var repo = RepoWith(
            Sms("111", "Alice", 1000, "Hello"),
            Sms("111", "Alice", 2000, "World"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationsAsync(AnyStream());

        Assert.Single(result);
        Assert.Equal(2, result[0].Messages.Count);
    }

    [Fact]
    public async Task When_MessagesFromDifferentContacts_Should_CreateSeparateConversations()
    {
        var repo = RepoWith(
            Sms("111", "Alice", 1000, "Hi"),
            Sms("222", "Bob", 2000, "Hey"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationsAsync(AnyStream());

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Should_SortConversationsByMostRecentMessageDescending()
    {
        var repo = RepoWith(
            Sms("111", "Alice", 1000, "old"),
            Sms("222", "Bob",   9000, "new"),
            Sms("111", "Alice", 5000, "middle"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationsAsync(AnyStream());

        Assert.Equal("222", result[0].Address);
        Assert.Equal("111", result[1].Address);
    }

    [Fact]
    public async Task Should_SortMessagesWithinConversationChronologically()
    {
        var repo = RepoWith(
            Sms("111", "Alice", 3000, "third"),
            Sms("111", "Alice", 1000, "first"),
            Sms("111", "Alice", 2000, "second"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationsAsync(AnyStream());

        Assert.Equal("first",  result[0].Messages[0].Body);
        Assert.Equal("second", result[0].Messages[1].Body);
        Assert.Equal("third",  result[0].Messages[2].Body);
    }

    [Fact]
    public async Task When_ContactNameIsNull_Should_UseAddressAsDisplayName()
    {
        var repo = RepoWith(Sms("555", "null", 1000, "hi"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationsAsync(AnyStream());

        Assert.Equal("555", result[0].DisplayName);
    }

    [Fact]
    public async Task When_ContactNameIsSet_Should_UseContactNameAsDisplayName()
    {
        var repo = RepoWith(Sms("555", "Alice", 1000, "hi"));
        var service = new ConversationService(repo.Object);

        var result = await service.GetConversationsAsync(AnyStream());

        Assert.Equal("Alice", result[0].DisplayName);
    }
}
