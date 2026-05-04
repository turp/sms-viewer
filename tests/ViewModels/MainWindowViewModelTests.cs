using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SmsViewer.Models;
using SmsViewer.Repositories;
using SmsViewer.Services;
using SmsViewer.ViewModels;
using Xunit;

namespace SmsViewer.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private static IMessage MakeSms(string contact = "Alice") =>
        new SmsMessage("555", 0, 1, "Hello", 1, -1, "Jan 1, 2000", contact);

    private static Mock<ISmsRepository> RepositoryWith(params IMessage[] messages)
    {
        var mock = new Mock<ISmsRepository>();
        mock.Setup(r => r.GetMessagesAsync(It.IsAny<Stream>()))
            .Returns(ToAsyncEnumerable(messages));
        return mock;
    }

    private static async IAsyncEnumerable<IMessage> ToAsyncEnumerable(
        IEnumerable<IMessage> items,
        [EnumeratorCancellation] CancellationToken _ = default)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }

    [Fact]
    public async Task When_FilePickerReturnsNull_Should_NotLoadMessages()
    {
        var picker = new Mock<IFilePickerService>();
        picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync((string?)null);
        var repo = new Mock<ISmsRepository>();

        var vm = new MainWindowViewModel(repo.Object, picker.Object);
        await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

        Assert.Empty(vm.Messages);
        repo.Verify(r => r.GetMessagesAsync(It.IsAny<Stream>()), Times.Never);
    }

    [Fact]
    public async Task When_RepositoryStreamsMessages_Should_PopulateCollection()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);

            var sms = MakeSms();
            var repo = RepositoryWith(sms);

            var vm = new MainWindowViewModel(repo.Object, picker.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

            Assert.Single(vm.Messages);
            Assert.Equal(sms, vm.Messages[0]);
        }
        finally
        {
            File.Delete(file);
        }
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
            var repo = RepositoryWith();

            var vm = new MainWindowViewModel(repo.Object, picker.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

            Assert.False(vm.IsLoading);
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public async Task When_FileDoesNotExist_Should_SetErrorMessage()
    {
        var picker = new Mock<IFilePickerService>();
        picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync("/nonexistent/path/backup.xml");
        var repo = new Mock<ISmsRepository>();

        var vm = new MainWindowViewModel(repo.Object, picker.Object);
        await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

        Assert.NotNull(vm.ErrorMessage);
        Assert.Empty(vm.Messages);
    }

    [Fact]
    public async Task When_NewFileLoaded_Should_ClearPreviousMessages()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);

            var repo = RepositoryWith(MakeSms("First"));
            var vm = new MainWindowViewModel(repo.Object, picker.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;
            Assert.Single(vm.Messages);

            repo.Setup(r => r.GetMessagesAsync(It.IsAny<Stream>()))
                .Returns(ToAsyncEnumerable(new[] { MakeSms("Second"), MakeSms("Third") }));
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

            Assert.Equal(2, vm.Messages.Count);
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public async Task When_RepositoryThrows_Should_SetErrorMessageAndClearIsLoading()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "<smses/>");
            var picker = new Mock<IFilePickerService>();
            picker.Setup(p => p.PickXmlFileAsync()).ReturnsAsync(file);

            var repo = new Mock<ISmsRepository>();
            repo.Setup(r => r.GetMessagesAsync(It.IsAny<Stream>()))
                .Returns(ThrowingAsyncEnumerable());

            var vm = new MainWindowViewModel(repo.Object, picker.Object);
            await vm.OpenXmlFileCommand.ExecuteAsync(null)!;

            Assert.NotNull(vm.ErrorMessage);
            Assert.False(vm.IsLoading);
        }
        finally
        {
            File.Delete(file);
        }
    }

    private static async IAsyncEnumerable<IMessage> ThrowingAsyncEnumerable()
    {
        await Task.Yield();
        throw new InvalidDataException("Corrupt XML");
#pragma warning disable CS0162
        yield break;
#pragma warning restore CS0162
    }
}
