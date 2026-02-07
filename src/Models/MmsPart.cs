namespace SmsViewer.Models;

/// <summary>
/// Represents a part of an MMS message (text, image, etc.).
/// </summary>
public record MmsPart(
    string ContentType,
    string Name,
    string Text,
    string Data);
