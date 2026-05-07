using System.Threading.Tasks;
using Moq;
using SmsViewer.Services;
using SmsViewer.Tests.Services;
using SmsViewer.ViewModels;
using Xunit;

namespace SmsViewer.Tests.ViewModels;

public class UpdateServiceTests
{
    private static MainWindowViewModel MakeVm(IUpdateService updateService) =>
        new(new Mock<IConversationService>().Object, new Mock<IFilePickerService>().Object, updateService);

    [Fact]
    public async Task When_UpdateAvailable_Should_SetUpdateAvailableTrue()
    {
        var fake = new FakeUpdateService(hasUpdate: true, version: "1.2.3");
        var vm = MakeVm(fake);

        await vm.UpdateCheckTask!;

        Assert.True(vm.UpdateAvailable);
        Assert.Equal("1.2.3", vm.AvailableVersion);
    }

    [Fact]
    public async Task When_NoUpdateAvailable_Should_LeaveUpdateAvailableFalse()
    {
        var fake = new FakeUpdateService(hasUpdate: false);
        var vm = MakeVm(fake);

        await vm.UpdateCheckTask!;

        Assert.False(vm.UpdateAvailable);
    }

    [Fact]
    public async Task When_Dismissed_Should_SetUpdateAvailableFalse()
    {
        var fake = new FakeUpdateService(hasUpdate: true, version: "2.0.0");
        var vm = MakeVm(fake);
        await vm.UpdateCheckTask!;
        Assert.True(vm.UpdateAvailable);

        vm.DismissUpdateCommand.Execute(null);

        Assert.False(vm.UpdateAvailable);
    }
}
