namespace SmsViewer.Models;

/// <summary>
/// Defines the core properties for any SMS or MMS message.
/// </summary>
public interface IMessage
{
    string Address { get; }
    long Date { get; }
    string Body { get; }
    string ReadableDate { get; }
    string ContactName { get; }
    bool IsSent { get; }
    bool IsReceived { get; }
    string DisplayBody { get; }
}
