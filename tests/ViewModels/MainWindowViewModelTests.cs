using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using SmsViewer.Models;
using SmsViewer.Services;
using SmsViewer.ViewModels;
using Xunit;
using System.Linq;

namespace SmsViewer.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private static ConversationSummary MakeSummary(string name = "Alice", string address = "555", long dateMs = 1000, int count = 1) =>
        new(address, name, "preview", "Jan 1", dateMs, count);

    private static Mock<IConversationService> ServiceWith(params ConversationSummary[] summaries)
    {
        var mock = new Mock<IConversationService>();
        mock.Setup(s => s.GetConversationSummariesAsync(It.IsAny<Stream>()))
            .ReturnsAsync((IReadOnlyList<ConversationSummary>)summaries);
        mock.Setup(s => s.GetConversationMessagesAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(new List<IMessage>());
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
        service.Verify(s => s.GetConversationSummariesAsync(It.IsAny<Stream>()), Times.Never);
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

            var vm = new MainWindowViewModel(ServiceWith(MakeSummary()).Object, picker.Object);
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

            var service = ServiceWith(MakeSummary("First"));
            var vm = new MainWindowViewModel(service.Object, picker.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
            Assert.Single(vm.Conversations);

            service.Setup(s => s.GetConversationSummariesAsync(It.IsAny<Stream>()))
                .ReturnsAsync(new[] { MakeSummary("A"), MakeSummary("B") });
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
            service.Setup(s => s.GetConversationSummariesAsync(It.IsAny<Stream>()))
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

            var vm = new MainWindowViewModel(ServiceWith(MakeSummary()).Object, picker.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
            vm.SelectedConversation = vm.Conversations[0];
            await vm.ThreadLoadTask!;

            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

            Assert.Null(vm.SelectedConversation);
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public async Task When_ConversationSelected_Should_LoadThreadMessages()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);

            var messages = new List<IMessage>
            {
                new SmsMessage("555", 1000, 1, "hello", 1, -1, "Jan 1", "Alice"),
                new SmsMessage("555", 2000, 1, "world", 1, -1, "Jan 1", "Alice"),
            };
            var service = ServiceWith(MakeSummary());
            service.Setup(s => s.GetConversationMessagesAsync(It.IsAny<Stream>(), "555"))
                .ReturnsAsync(messages);

            var vm = new MainWindowViewModel(service.Object, picker.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
            vm.SelectedConversation = vm.Conversations[0];
            await vm.ThreadLoadTask!;

            Assert.Equal(2, vm.FilteredMessages.Count);
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public async Task EmptyThreadState_Before_ConversationSelected()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);

            var vm = new MainWindowViewModel(ServiceWith(MakeSummary()).Object, picker.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

            Assert.Empty(vm.FilteredMessages);
            Assert.Null(vm.SelectedConversation);
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public async Task ExportSelectedCommand_DisabledBeforeAnyConversationChecked()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);

            var vm = new MainWindowViewModel(ServiceWith(MakeSummary()).Object, picker.Object, null, new Mock<IExportService>().Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

            Assert.False(vm.ExportSelectedCommand.CanExecute(null));
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public async Task ExportSelectedCommand_EnabledWhenConversationChecked()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);

            var vm = new MainWindowViewModel(ServiceWith(MakeSummary()).Object, picker.Object, null, new Mock<IExportService>().Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
            vm.Conversations[0].IsSelected = true;

            Assert.True(vm.ExportSelectedCommand.CanExecute(null));
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public async Task ExportSelectedCommand_WhenSaveDialogCancelled_DoesNotCallExportService()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);
            picker.Setup(p => p.PickSaveXmlFileAsync(It.IsAny<string>())).ReturnsAsync((string?)null);

            var exportService = new Mock<IExportService>();
            var vm = new MainWindowViewModel(ServiceWith(MakeSummary()).Object, picker.Object, null, exportService.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
            vm.Conversations[0].IsSelected = true;

            await vm.ExportSelectedCommand.ExecuteAsync(null)!;

            exportService.Verify(
                e => e.ExportThreadsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<Stream>()),
                Times.Never);
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public async Task ExportSelectedCommand_WhenExportServiceThrows_SetsErrorMessage()
    {
        var file = Path.GetTempFileName();
        var saveFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);
            picker.Setup(p => p.PickSaveXmlFileAsync(It.IsAny<string>())).ReturnsAsync(saveFile);

            var exportService = new Mock<IExportService>();
            exportService
                .Setup(e => e.ExportThreadsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<Stream>()))
                .ThrowsAsync(new IOException("Disk full"));

            var vm = new MainWindowViewModel(ServiceWith(MakeSummary()).Object, picker.Object, null, exportService.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
            vm.Conversations[0].IsSelected = true;

            await vm.ExportSelectedCommand.ExecuteAsync(null)!;

            Assert.NotNull(vm.ErrorMessage);
        }
        finally
        {
            File.Delete(file);
            try { File.Delete(saveFile); } catch { }
        }
    }

    [Fact]
    public async Task ExportSelectedCommand_PassesSelectedAddressesToExportService()
    {
        var file = Path.GetTempFileName();
        var saveFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);
            picker.Setup(p => p.PickSaveXmlFileAsync(It.IsAny<string>())).ReturnsAsync(saveFile);

            IEnumerable<string>? capturedAddresses = null;
            var exportService = new Mock<IExportService>();
            exportService
                .Setup(e => e.ExportThreadsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<Stream>()))
                .Callback<string, IEnumerable<string>, Stream>((_, addrs, _) => capturedAddresses = addrs)
                .Returns(Task.CompletedTask);

            var vm = new MainWindowViewModel(
                ServiceWith(MakeSummary("Alice", "111"), MakeSummary("Bob", "222")).Object,
                picker.Object, null, exportService.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
            vm.Conversations[0].IsSelected = true; // Alice

            await vm.ExportSelectedCommand.ExecuteAsync(null)!;

            Assert.NotNull(capturedAddresses);
            Assert.Single(capturedAddresses!);
            Assert.Contains("111", capturedAddresses!);
        }
        finally
        {
            File.Delete(file);
            try { File.Delete(saveFile); } catch { }
        }
    }
}
