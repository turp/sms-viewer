using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace SmsViewer.Services;

public static class ThemeService
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SmsViewer", "settings.json");

    public static IReadOnlyList<ThemeDefinition> AvailableThemes { get; } =
    [
        new("Ocean Light",    "avares://SmsViewer/Themes/OceanTheme.axaml",    false, "#0078D4"),
        new("Ocean Dark",     "avares://SmsViewer/Themes/OceanTheme.axaml",    true,  "#0078D4"),
        new("Forest Light",   "avares://SmsViewer/Themes/ForestTheme.axaml",   false, "#2E7D32"),
        new("Forest Dark",    "avares://SmsViewer/Themes/ForestTheme.axaml",   true,  "#2E7D32"),
        new("Sunset Light",   "avares://SmsViewer/Themes/SunsetTheme.axaml",   false, "#E64A19"),
        new("Sunset Dark",    "avares://SmsViewer/Themes/SunsetTheme.axaml",   true,  "#E64A19"),
        new("Lavender Light", "avares://SmsViewer/Themes/LavenderTheme.axaml", false, "#7B1FA2"),
        new("Lavender Dark",  "avares://SmsViewer/Themes/LavenderTheme.axaml", true,  "#7B1FA2"),
    ];

    public static ThemeDefinition Current { get; private set; } = AvailableThemes[0];

    public static ThemeDefinition Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                var saved = AvailableThemes.FirstOrDefault(t => t.Name == settings?.Theme);
                if (saved != null) return saved;
            }
        }
        catch { }
        return AvailableThemes[0];
    }

    public static void Apply(ThemeDefinition theme)
    {
        var app = Application.Current!;
        app.RequestedThemeVariant = theme.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;

        // Set accent color via FluentTheme.Palettes — the correct API for Avalonia 11
        var fluentTheme = app.Styles.OfType<FluentTheme>().FirstOrDefault();
        if (fluentTheme != null)
        {
            var accent = Color.Parse(theme.AccentHex);
            fluentTheme.Palettes[ThemeVariant.Light] = new ColorPaletteResources { Accent = accent };
            fluentTheme.Palettes[ThemeVariant.Dark]  = new ColorPaletteResources { Accent = accent };
        }

        // Swap theme-specific resource overrides (sent bubble brushes)
        var mds = app.Resources.MergedDictionaries;
        mds.Clear();
        mds.Add(new ResourceInclude((Uri?)null) { Source = new Uri(theme.ResourcePath) });

        Current = theme;
        Save(theme);
    }

    private static void Save(ThemeDefinition theme)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(new AppSettings { Theme = theme.Name }));
        }
        catch { }
    }

    private sealed class AppSettings
    {
        public string? Theme { get; set; }
    }
}
