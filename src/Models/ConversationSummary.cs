namespace SmsViewer.Models;

public record ConversationSummary(
    string Address,
    string ContactName,
    string LastMessagePreview,
    string LastMessageDate,
    long LastMessageDateUnixMs,
    int MessageCount)
{
    public string DisplayName =>
        string.IsNullOrWhiteSpace(ContactName) || ContactName == "null" || ContactName == "(Unknown)"
            ? Address
            : ContactName;
}
