using System.Collections.Generic;

namespace SmsViewer.Models;

/// <summary>
/// Aggregates all messages exchanged with a single contact.
/// </summary>
public record Conversation(
    string Address,
    string ContactName,
    IReadOnlyList<IMessage> Messages)
{
    public string DisplayName =>
        string.IsNullOrWhiteSpace(ContactName) || ContactName == "null"
            ? Address
            : ContactName;
}
