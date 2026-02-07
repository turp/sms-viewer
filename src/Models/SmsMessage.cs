namespace SmsViewer.Models;

/// <summary>
/// Represents a standard SMS message.
/// </summary>
public record SmsMessage(
    string Address,
    long Date,
    int Type,
    string Body,
    int Read,
    int Status,
    string ReadableDate,
    string ContactName) : IMessage;
