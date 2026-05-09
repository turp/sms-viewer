using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using SmsViewer.Repositories;
using SmsViewer.Services;
using SmsViewer.ViewModels;
using SmsViewer.Views;

namespace SmsViewer;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            ThemeService.Apply(ThemeService.Load());

            var mainWindow = new MainWindow();
            var repository = new XmlSmsRepository();
            var conversationService = new ConversationService(repository);
            var filePickerService = new FilePickerService(mainWindow.StorageProvider);
            var updateService = new UpdateService();
            var exportService = new ExportService();

            mainWindow.DataContext = new MainWindowViewModel(conversationService, filePickerService, updateService, exportService);
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}