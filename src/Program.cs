using Avalonia;
using System;
using Velopack;

namespace SmsViewer;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Velopack must run before any Avalonia or app initialization
        VelopackApp.Build().Run();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
