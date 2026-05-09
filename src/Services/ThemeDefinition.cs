namespace SmsViewer.Services;

public record ThemeDefinition(string Name, string ResourcePath, bool IsDark, string AccentHex)
{
    public override string ToString() => Name;
}
