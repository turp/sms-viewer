using System;
using System.Collections.Generic;
using System.IO;
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
    private const long JunMs  = 1686787200000L;
    // Unix ms for 2023-01-01 00:00:00 UTC
    private const long JanMs  = 1672531200000L;
    // Unix ms for 2023-12-31 00:00:00 UTC
    private const long DecMs  = 1703980800000L;

    private static Conversation Conv(string address, string name, long dateMs, string body = "hi") =>
        new(address, name, new[] { new SmsMessage(address, dateMs, 1, body, 1, -1, "date", name) });

    private static async Task<MainWindowViewModel> LoadedVm(params Conversation[] conversations)
    {
        var file = Path.GetTempFileName();
        File.WriteAllText(file, "<smses/>");
        var picker = new Mock<IFilePickerService>();
        picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);
        var service = new Mock<IConversationService>();
        service.Setup(s => s.GetConversationsAsync(It.IsAny<Stream>()))
               .ReturnsAsync((IReadOnlyList<Conversation>)conversations);
        var vm = new MainWindowViewModel(service.Object, picker.Object);
        await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
        File.Delete(file);
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
            Conv("222", "Bob",   JunMs),
            Conv("333", "Carol", DecMs));
        vm.FilterFromDate = "2023-03-01";
        vm.FilterToDate   = "2023-09-01";
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
        vm.SearchText     = "alice";
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

        // Load again (empty result)
        var file = Path.GetTempFileName();
        File.WriteAllText(file, "<smses/>");
        var picker = new Mock<IFilePickerService>();
        picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);
        var service = new Mock<IConversationService>();
        service.Setup(s => s.GetConversationsAsync(It.IsAny<Stream>()))
               .ReturnsAsync(Array.Empty<Conversation>());
        var vm2 = new MainWindowViewModel(service.Object, picker.Object);
        vm2.SearchText = "alice";
        await vm2.OpenXmlFileCommand.ExecuteAsync(null)!;
        File.Delete(file);

        Assert.Equal(string.Empty, vm2.SearchText);
        Assert.Equal(string.Empty, vm2.FilterFromDate);
        Assert.Equal(string.Empty, vm2.FilterToDate);
    }

    // ── Message filter ────────────────────────────────────────────────────

    private static Conversation ConvWithMessages(params string[] bodies)
    {
        var messages = new List<IMessage>();
        for (var i = 0; i < bodies.Length; i++)
            messages.Add(new SmsMessage("111", i * 1000L, 1, bodies[i], 1, -1, "date", "Alice"));
        return new Conversation("111", "Alice", messages);
    }

    [Fact]
    public async Task When_ConversationSelected_FilteredMessages_ShowsAll()
    {
        var conv = ConvWithMessages("hello", "world");
        var vm = await LoadedVm(conv);
        vm.SelectedConversation = vm.Conversations[0];
        Assert.Equal(2, vm.FilteredMessages.Count);
    }

    [Fact]
    public async Task When_ThreadSearchSet_ShowsOnlyMatchingMessages()
    {
        var vm = await LoadedVm(ConvWithMessages("hello world", "goodbye"));
        vm.SelectedConversation = vm.Conversations[0];
        vm.ThreadSearchText = "hello";
        Assert.Single(vm.FilteredMessages);
        Assert.Equal("hello world", vm.FilteredMessages[0].Body);
    }

    [Fact]
    public async Task ThreadSearch_IsCaseInsensitive()
    {
        var vm = await LoadedVm(ConvWithMessages("Hello World"));
        vm.SelectedConversation = vm.Conversations[0];
        vm.ThreadSearchText = "HELLO";
        Assert.Single(vm.FilteredMessages);
    }

    [Fact]
    public async Task When_ThreadSearchMatchesNone_FilteredMessagesIsEmpty()
    {
        var vm = await LoadedVm(ConvWithMessages("hello"));
        vm.SelectedConversation = vm.Conversations[0];
        vm.ThreadSearchText = "zzznomatch";
        Assert.Empty(vm.FilteredMessages);
    }

    [Fact]
    public async Task When_ConversationChanges_ThreadSearchIsReset()
    {
        var vm = await LoadedVm(
            ConvWithMessages("hello"),
            ConvWithMessages("world"));
        vm.SelectedConversation = vm.Conversations[0];
        vm.ThreadSearchText = "hello";
        vm.SelectedConversation = vm.Conversations[1];
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
        var conv = ConvWithMessages("hello");
        var service = new Mock<IConversationService>();
        service.Setup(s => s.GetConversationsAsync(It.IsAny<Stream>()))
               .ReturnsAsync(new[] { conv });
        var vm = new MainWindowViewModel(service.Object, picker.Object);
        await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
        vm.SelectedConversation = vm.Conversations[0];
        vm.ThreadSearchText = "hello";

        service.Setup(s => s.GetConversationsAsync(It.IsAny<Stream>()))
               .ReturnsAsync(Array.Empty<Conversation>());
        await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
        File.Delete(file);

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
        var vm = await LoadedVm(ConvWithMessages("hello"));
        vm.SelectedConversation = vm.Conversations[0];
        vm.ThreadSearchText = "zzznomatch";
        Assert.True(vm.HasNoMessageResults);
    }
}
