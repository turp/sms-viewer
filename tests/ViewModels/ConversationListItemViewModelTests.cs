using SmsViewer.Models;
using SmsViewer.ViewModels;
using Xunit;

namespace SmsViewer.Tests.ViewModels;

public class ConversationListItemViewModelTests
{
    private static ConversationSummary Summary(
        string address = "555",
        string contactName = "Alice",
        string preview = "Hi",
        string date = "Jan 1",
        long dateMs = 1000,
        int count = 1) =>
        new(address, contactName, preview, date, dateMs, count);

    [Fact]
    public void DisplayName_Should_BeContactName_WhenSet()
    {
        var vm = new ConversationListItemViewModel(Summary(contactName: "Alice"));
        Assert.Equal("Alice", vm.DisplayName);
    }

    [Fact]
    public void DisplayName_Should_FallBackToAddress_WhenContactNameIsNullString()
    {
        var vm = new ConversationListItemViewModel(Summary(address: "555", contactName: "null"));
        Assert.Equal("555", vm.DisplayName);
    }

    [Fact]
    public void LastMessagePreview_Should_ExposePreviewFromSummary()
    {
        var vm = new ConversationListItemViewModel(Summary(preview: "Last message"));
        Assert.Equal("Last message", vm.LastMessagePreview);
    }

    [Fact]
    public void MessageCount_Should_ReflectSummaryCount()
    {
        var vm = new ConversationListItemViewModel(Summary(count: 3));
        Assert.Equal(3, vm.MessageCount);
    }

    [Fact]
    public void Address_Should_ExposeAddressFromSummary()
    {
        var vm = new ConversationListItemViewModel(Summary(address: "999"));
        Assert.Equal("999", vm.Address);
    }

    [Fact]
    public void LastMessageDateUnixMs_Should_ExposeFromSummary()
    {
        var vm = new ConversationListItemViewModel(Summary(dateMs: 99999L));
        Assert.Equal(99999L, vm.LastMessageDateUnixMs);
    }
}
