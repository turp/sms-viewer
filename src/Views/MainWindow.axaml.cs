using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using SmsViewer.Models;

namespace SmsViewer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void UrlText_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left &&
            sender is TextBlock { DataContext: BodySegment { IsUrl: true, Text: var url } })
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { /* best-effort: silently ignore if the OS can't open the URL */ }
        }
    }
}
