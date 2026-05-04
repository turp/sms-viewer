using System.Collections.Generic;
using SmsViewer.Models;
using SmsViewer.ViewModels;
using Xunit;

namespace SmsViewer.Tests.ViewModels;

public class ConversationListItemViewModelTests
{
    private static IMessage Sms(long date, string body, string contact = "Alice") =>
        new SmsMessage("555", date, 1, body, 1, -1, "Jan 1", contact);

    [Fact]
    public void DisplayName_Should_BeContactName_WhenSet()
    {
        var conversation = new Conversation("555", "Alice", new[] { Sms(1000, "Hi") });
        var vm = new ConversationListItemViewModel(conversation);
        Assert.Equal("Alice", vm.DisplayName);
    }

    [Fact]
    public void DisplayName_Should_FallBackToAddress_WhenContactNameIsNull_String()
    {
        var conversation = new Conversation("555", "null", new[] { Sms(1000, "Hi", "null") });
        var vm = new ConversationListItemViewModel(conversation);
        Assert.Equal("555", vm.DisplayName);
    }

    [Fact]
    public void LastMessagePreview_Should_BeLastMessageBody()
    {
        var conversation = new Conversation("555", "Alice", new[]
        {
            Sms(1000, "First"),
            Sms(2000, "Last")
        });
        var vm = new ConversationListItemViewModel(conversation);
        Assert.Equal("Last", vm.LastMessagePreview);
    }

    [Fact]
    public void LastMessagePreview_Should_BeTruncatedAt60Chars()
    {
        var longBody = new string('x', 80);
        var conversation = new Conversation("555", "Alice", new[] { Sms(1000, longBody) });
        var vm = new ConversationListItemViewModel(conversation);
        Assert.Equal(61, vm.LastMessagePreview.Length); // 60 chars + ellipsis (1 char)
        Assert.EndsWith("…", vm.LastMessagePreview);
    }

    [Fact]
    public void MessageCount_Should_ReflectNumberOfMessages()
    {
        var conversation = new Conversation("555", "Alice", new[]
        {
            Sms(1000, "a"),
            Sms(2000, "b"),
            Sms(3000, "c")
        });
        var vm = new ConversationListItemViewModel(conversation);
        Assert.Equal(3, vm.MessageCount);
    }

    [Fact]
    public void Messages_Should_ExposeSameCollectionAsConversation()
    {
        var messages = new List<IMessage> { Sms(1000, "hi") };
        var conversation = new Conversation("555", "Alice", messages);
        var vm = new ConversationListItemViewModel(conversation);
        Assert.Same(messages, vm.Messages);
    }
}
