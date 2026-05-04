using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using SmsViewer.Models;
using SmsViewer.Services;
using SmsViewer.ViewModels;
using Xunit;

namespace SmsViewer.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private static Conversation MakeConversation(string name = "Alice", int count = 1)
    {
        var messages = new List<IMessage>();
        for (var i = 0; i < count; i++)
            messages.Add(new SmsMessage("555", i * 1000L, 1, $"msg {i}", 1, -1, "Jan 1", name));
        return new Conversation("555", name, messages);
    }

    private static Mock<IConversationService> ServiceWith(params Conversation[] conversations)
    {
        var mock = new Mock<IConversationService>();
        mock.Setup(s => s.GetConversationsAsync(It.IsAny<Stream>()))
            .ReturnsAsync((IReadOnlyList<Conversation>)conversations);
        return mock;
    }

    [Fact]
    public async Task When_FilePickerReturnsNull_Should_NotLoadConversations()
    {
        var picker = new Mock<IFilePickerService>();
        picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync((string?)null);
        var service = new Mock<IConversationService>();

        var vm = new MainWindowViewModel(service.Object, picker.Object);
        await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

        Assert.Empty(vm.Conversations);
        service.Verify(s => s.GetConversationsAsync(It.IsAny<Stream>()), Times.Never);
    }

    [Fact]
    public async Task When_ServiceReturnsConversations_Should_PopulateCollection()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);

            var vm = new MainWindowViewModel(ServiceWith(MakeConversation()).Object, picker.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

            Assert.Single(vm.Conversations);
            Assert.Equal("Alice", vm.Conversations[0].DisplayName);
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public async Task When_LoadCompletes_Should_SetIsLoadingFalse()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);

            var vm = new MainWindowViewModel(ServiceWith().Object, picker.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

            Assert.False(vm.IsLoading);
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public async Task When_FileDoesNotExist_Should_SetErrorMessage()
    {
        var picker = new Mock<IFilePickerService>();
        picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync("/nonexistent/path/backup.xml");

        var vm = new MainWindowViewModel(new Mock<IConversationService>().Object, picker.Object);
        await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

        Assert.NotNull(vm.ErrorMessage);
        Assert.Empty(vm.Conversations);
    }

    [Fact]
    public async Task When_NewFileLoaded_Should_ClearPreviousConversations()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);

            var service = ServiceWith(MakeConversation("First"));
            var vm = new MainWindowViewModel(service.Object, picker.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
            Assert.Single(vm.Conversations);

            service.Setup(s => s.GetConversationsAsync(It.IsAny<Stream>()))
                .ReturnsAsync(new[] { MakeConversation("A"), MakeConversation("B") });
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

            Assert.Equal(2, vm.Conversations.Count);
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public async Task When_ServiceThrows_Should_SetErrorMessageAndClearIsLoading()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);

            var service = new Mock<IConversationService>();
            service.Setup(s => s.GetConversationsAsync(It.IsAny<Stream>()))
                .ThrowsAsync(new InvalidDataException("Corrupt XML"));

            var vm = new MainWindowViewModel(service.Object, picker.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

            Assert.NotNull(vm.ErrorMessage);
            Assert.False(vm.IsLoading);
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public async Task When_NewFileLoaded_Should_ClearSelectedConversation()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);

            var vm = new MainWindowViewModel(ServiceWith(MakeConversation()).Object, picker.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
            vm.SelectedConversation = vm.Conversations[0];

            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

            Assert.Null(vm.SelectedConversation);
        }
        finally { File.Delete(file); }
    }
}
