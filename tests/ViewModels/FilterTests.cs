using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SmsViewer.Models;
using SmsViewer.Services;
using SmsViewer.ViewModels;
using Xunit;

namespace SmsViewer.Tests.ViewModels;

/// <summary>
/// Tests for conversation and message filter behaviour in MainWindowViewModel.
/// </summary>
public class FilterTests
{
    // Unix ms for 2023-06-15 00:00:00 UTC
    private const long JunMs = 1686787200000L;
    // Unix ms for 2023-01-01 00:00:00 UTC
    private const long JanMs = 1672531200000L;
    // Unix ms for 2023-12-31 00:00:00 UTC
    private const long DecMs = 1703980800000L;

    private static ConversationSummary Conv(string address, string name, long dateMs, string preview = "hi") =>
        new(address, name, preview, "date", dateMs, 1);

    private static (ConversationSummary summary, IReadOnlyList<IMessage> messages) ConvWithMessages(
        string address, params string[] bodies)
    {
        var msgs = bodies.Select((b, i) =>
            (IMessage)new SmsMessage(address, i * 1000L, 1, b, 1, -1, "date", "Alice")).ToList();
        var summary = new ConversationSummary(address, "Alice", bodies[^1], "date", (bodies.Length - 1) * 1000L, bodies.Length);
        return (summary, msgs);
    }

    private static async Task<MainWindowViewModel> LoadedVm(params ConversationSummary[] summaries)
    {
        return await LoadedVmWithMessages(summaries.Select(s => (s, (IReadOnlyList<IMessage>)new List<IMessage>())).ToArray());
    }

    private static async Task<MainWindowViewModel> LoadedVmWithMessages(
        params (ConversationSummary summary, IReadOnlyList<IMessage> messages)[] entries)
    {
        var file = Path.GetTempFileName();
        File.WriteAllText(file, "<smses/>");
        var picker = new Mock<IFilePickerService>();
        picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);
        var service = new Mock<IConversationService>();
        service.Setup(s => s.GetConversationSummariesAsync(It.IsAny<Stream>()))
               .ReturnsAsync(entries.Select(e => e.summary).ToList());
        foreach (var (summary, messages) in entries)
        {
            service.Setup(s => s.GetConversationMessagesAsync(It.IsAny<Stream>(), summary.Address))
                   .ReturnsAsync(messages);
        }
        var vm = new MainWindowViewModel(service.Object, picker.Object);
        await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
        // Note: file is kept alive — LoadSelectedThreadAsync reopens it when a conversation is selected.
        // OS will clean it up from the temp directory eventually.
        return vm;
    }

    // ── Conversation filter ───────────────────────────────────────────────

    [Fact]
    public async Task When_SearchTextEmpty_FilteredConversations_ShowsAll()
    {
        var vm = await LoadedVm(Conv("111", "Alice", JunMs), Conv("222", "Bob", JunMs));
        Assert.Equal(2, vm.FilteredConversations.Count);
    }

    [Fact]
    public async Task When_SearchTextMatchesName_ShowsMatchingConversation()
    {
        var vm = await LoadedVm(Conv("111", "Alice", JunMs), Conv("222", "Bob", JunMs));
        vm.SearchText = "alice";
        Assert.Single(vm.FilteredConversations);
        Assert.Equal("Alice", vm.FilteredConversations[0].DisplayName);
    }

    [Fact]
    public async Task When_SearchTextMatchesAddress_ShowsMatchingConversation()
    {
        var vm = await LoadedVm(Conv("111", "Alice", JunMs), Conv("222", "Bob", JunMs));
        vm.SearchText = "222";
        Assert.Single(vm.FilteredConversations);
        Assert.Equal("222", vm.FilteredConversations[0].Address);
    }

    [Fact]
    public async Task When_SearchTextMatchesNone_FilteredConversationsIsEmpty()
    {
        var vm = await LoadedVm(Conv("111", "Alice", JunMs));
        vm.SearchText = "zzznomatch";
        Assert.Empty(vm.FilteredConversations);
    }

    [Fact]
    public async Task When_SearchTextCleared_ShowsAllConversations()
    {
        var vm = await LoadedVm(Conv("111", "Alice", JunMs), Conv("222", "Bob", JunMs));
        vm.SearchText = "alice";
        vm.SearchText = string.Empty;
        Assert.Equal(2, vm.FilteredConversations.Count);
    }

    [Fact]
    public async Task SearchText_IsCaseInsensitive()
    {
        var vm = await LoadedVm(Conv("111", "Alice", JunMs));
        vm.SearchText = "ALICE";
        Assert.Single(vm.FilteredConversations);
    }

    [Fact]
    public async Task When_FromDateSet_ExcludesConversationsBeforeIt()
    {
        var vm = await LoadedVm(Conv("111", "Alice", JanMs), Conv("222", "Bob", DecMs));
        vm.FilterFromDate = "2023-06-01";
        Assert.Single(vm.FilteredConversations);
        Assert.Equal("222", vm.FilteredConversations[0].Address);
    }

    [Fact]
    public async Task When_ToDateSet_ExcludesConversationsAfterIt()
    {
        var vm = await LoadedVm(Conv("111", "Alice", JanMs), Conv("222", "Bob", DecMs));
        vm.FilterToDate = "2023-06-01";
        Assert.Single(vm.FilteredConversations);
        Assert.Equal("111", vm.FilteredConversations[0].Address);
    }

    [Fact]
    public async Task When_BothDatesSet_OnlyShowsConversationsInRange()
    {
        var vm = await LoadedVm(
            Conv("111", "Alice", JanMs),
            Conv("222", "Bob", JunMs),
            Conv("333", "Carol", DecMs));
        vm.FilterFromDate = "2023-03-01";
        vm.FilterToDate = "2023-09-01";
        Assert.Single(vm.FilteredConversations);
        Assert.Equal("222", vm.FilteredConversations[0].Address);
    }

    [Fact]
    public async Task When_InvalidDateEntered_DateBoundIsIgnored()
    {
        var vm = await LoadedVm(Conv("111", "Alice", JunMs), Conv("222", "Bob", DecMs));
        vm.FilterFromDate = "not-a-date";
        Assert.Equal(2, vm.FilteredConversations.Count);
    }

    [Fact]
    public async Task FiltersAreAnded_TextAndDateMustBothMatch()
    {
        var vm = await LoadedVm(
            Conv("111", "Alice", JanMs),
            Conv("222", "Alice", DecMs));
        vm.SearchText = "alice";
        vm.FilterFromDate = "2023-06-01";
        Assert.Single(vm.FilteredConversations);
        Assert.Equal("222", vm.FilteredConversations[0].Address);
    }

    [Fact]
    public async Task When_NewFileLoaded_FiltersAreReset()
    {
        var vm = await LoadedVm(Conv("111", "Alice", JunMs));
        vm.SearchText = "alice";
        Assert.Single(vm.FilteredConversations);

        var file = Path.GetTempFileName();
        File.WriteAllText(file, "<smses/>");
        var picker = new Mock<IFilePickerService>();
        picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);
        var service = new Mock<IConversationService>();
        service.Setup(s => s.GetConversationSummariesAsync(It.IsAny<Stream>()))
               .ReturnsAsync(Array.Empty<ConversationSummary>());
        var vm2 = new MainWindowViewModel(service.Object, picker.Object);
        vm2.SearchText = "alice";
        await vm2.OpenXmlFileCommand.ExecuteAsync(null)!;
        File.Delete(file);

        Assert.Equal(string.Empty, vm2.SearchText);
        Assert.Equal(string.Empty, vm2.FilterFromDate);
        Assert.Equal(string.Empty, vm2.FilterToDate);
    }

    // ── Message filter ────────────────────────────────────────────────────

    [Fact]
    public async Task When_ConversationSelected_FilteredMessages_ShowsAll()
    {
        var (summary, messages) = ConvWithMessages("111", "hello", "world");
        var vm = await LoadedVmWithMessages((summary, messages));
        vm.SelectedConversation = vm.Conversations[0];
        await vm.ThreadLoadTask!;
        Assert.Equal(2, vm.FilteredMessages.Count);
    }

    [Fact]
    public async Task When_ThreadSearchSet_ShowsOnlyMatchingMessages()
    {
        var (summary, messages) = ConvWithMessages("111", "hello world", "goodbye");
        var vm = await LoadedVmWithMessages((summary, messages));
        vm.SelectedConversation = vm.Conversations[0];
        await vm.ThreadLoadTask!;
        vm.ThreadSearchText = "hello";
        Assert.Single(vm.FilteredMessages);
        Assert.Equal("hello world", vm.FilteredMessages[0].Body);
    }

    [Fact]
    public async Task ThreadSearch_IsCaseInsensitive()
    {
        var (summary, messages) = ConvWithMessages("111", "Hello World");
        var vm = await LoadedVmWithMessages((summary, messages));
        vm.SelectedConversation = vm.Conversations[0];
        await vm.ThreadLoadTask!;
        vm.ThreadSearchText = "HELLO";
        Assert.Single(vm.FilteredMessages);
    }

    [Fact]
    public async Task When_ThreadSearchMatchesNone_FilteredMessagesIsEmpty()
    {
        var (summary, messages) = ConvWithMessages("111", "hello");
        var vm = await LoadedVmWithMessages((summary, messages));
        vm.SelectedConversation = vm.Conversations[0];
        await vm.ThreadLoadTask!;
        vm.ThreadSearchText = "zzznomatch";
        Assert.Empty(vm.FilteredMessages);
    }

    [Fact]
    public async Task When_ConversationChanges_ThreadSearchIsReset()
    {
        var (s1, m1) = ConvWithMessages("111", "hello");
        var (s2, m2) = ConvWithMessages("222", "world");
        var vm = await LoadedVmWithMessages((s1, m1), (s2, m2));
        vm.SelectedConversation = vm.Conversations[0];
        await vm.ThreadLoadTask!;
        vm.ThreadSearchText = "hello";
        vm.SelectedConversation = vm.Conversations[1];
        await vm.ThreadLoadTask!;
        Assert.Equal(string.Empty, vm.ThreadSearchText);
        Assert.Single(vm.FilteredMessages);
    }

    [Fact]
    public async Task When_NewFileLoaded_ThreadSearchIsReset()
    {
        var file = Path.GetTempFileName();
        File.WriteAllText(file, "<smses/>");
        var picker = new Mock<IFilePickerService>();
        picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);
        var (summary, messages) = ConvWithMessages("111", "hello");
        var service = new Mock<IConversationService>();
        service.Setup(s => s.GetConversationSummariesAsync(It.IsAny<Stream>()))
               .ReturnsAsync(new[] { summary });
        service.Setup(s => s.GetConversationMessagesAsync(It.IsAny<Stream>(), "111"))
               .ReturnsAsync(messages);
        var vm = new MainWindowViewModel(service.Object, picker.Object);
        await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
        vm.SelectedConversation = vm.Conversations[0];
        await vm.ThreadLoadTask!;
        vm.ThreadSearchText = "hello";

        service.Setup(s => s.GetConversationSummariesAsync(It.IsAny<Stream>()))
               .ReturnsAsync(Array.Empty<ConversationSummary>());
        await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

        try { File.Delete(file); } catch { /* best-effort cleanup */ }
        Assert.Equal(string.Empty, vm.ThreadSearchText);
    }

    // ── HasNo*Results ─────────────────────────────────────────────────────

    [Fact]
    public async Task HasNoConversationResults_TrueWhenFilteredListEmpty()
    {
        var vm = await LoadedVm(Conv("111", "Alice", JunMs));
        vm.SearchText = "zzznomatch";
        Assert.True(vm.HasNoConversationResults);
    }

    [Fact]
    public async Task HasNoConversationResults_FalseWhenResultsExist()
    {
        var vm = await LoadedVm(Conv("111", "Alice", JunMs));
        Assert.False(vm.HasNoConversationResults);
    }

    [Fact]
    public async Task HasNoMessageResults_TrueWhenFilteredMessagesEmpty()
    {
        var (summary, messages) = ConvWithMessages("111", "hello");
        var vm = await LoadedVmWithMessages((summary, messages));
        vm.SelectedConversation = vm.Conversations[0];
        await vm.ThreadLoadTask!;
        vm.ThreadSearchText = "zzznomatch";
        Assert.True(vm.HasNoMessageResults);
    }

    [Fact]
    public async Task ContactFilter_DoesNotRequireNonSelectedThreadsToLoad()
    {
        // Contact filter operates over summaries — thread service should only be called when a conversation is selected
        var file = Path.GetTempFileName();
        File.WriteAllText(file, "<smses/>");
        var picker = new Mock<IFilePickerService>();
        picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);
        var service = new Mock<IConversationService>();
        service.Setup(s => s.GetConversationSummariesAsync(It.IsAny<Stream>()))
               .ReturnsAsync(new[]
               {
                   Conv("111", "Alice", JunMs),
                   Conv("222", "Bob", JunMs),
               });
        service.Setup(s => s.GetConversationMessagesAsync(It.IsAny<Stream>(), It.IsAny<string>()))
               .ReturnsAsync(new List<IMessage>());

        var vm = new MainWindowViewModel(service.Object, picker.Object);
        await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
        vm.SearchText = "alice";
        File.Delete(file);

        Assert.Single(vm.FilteredConversations);
        // GetConversationMessagesAsync should NOT have been called during contact filtering
        service.Verify(s => s.GetConversationMessagesAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
    }
}
