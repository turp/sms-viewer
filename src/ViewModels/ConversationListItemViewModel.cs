using System.Collections.Generic;
using SmsViewer.Models;

namespace SmsViewer.ViewModels;

public class ConversationListItemViewModel
{
    public string Address { get; }
    public string DisplayName { get; }
    public string LastMessagePreview { get; }
    public string LastMessageDate { get; }
    public long LastMessageDateUnixMs { get; }
    public int MessageCount { get; }
    public IReadOnlyList<IMessage> Messages { get; }

    public ConversationListItemViewModel(Conversation conversation)
    {
        var last = conversation.Messages[^1];
        Address = conversation.Address;
        DisplayName = conversation.DisplayName;
        LastMessageDate = last.ReadableDate;
        LastMessageDateUnixMs = last.Date;
        MessageCount = conversation.Messages.Count;
        Messages = conversation.Messages;

        var body = last.DisplayBody;
        LastMessagePreview = body.Length > 60 ? body[..60] + "…" : body;
    }
}
