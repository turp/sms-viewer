using System.Collections.Generic;
using SmsViewer.Models;

namespace SmsViewer.ViewModels;

public class ConversationListItemViewModel
{
    public string DisplayName { get; }
    public string LastMessagePreview { get; }
    public string LastMessageDate { get; }
    public int MessageCount { get; }
    public IReadOnlyList<IMessage> Messages { get; }

    public ConversationListItemViewModel(Conversation conversation)
    {
        var last = conversation.Messages[^1];
        DisplayName = conversation.DisplayName;
        LastMessageDate = last.ReadableDate;
        MessageCount = conversation.Messages.Count;
        Messages = conversation.Messages;

        var body = last.Body;
        LastMessagePreview = body.Length > 60 ? body[..60] + "…" : body;
    }
}
