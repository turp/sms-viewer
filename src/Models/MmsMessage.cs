using System.Collections.Generic;

namespace SmsViewer.Models;

/// <summary>
/// Represents a multi-media message (MMS).
/// </summary>
public record MmsMessage(
    string Address,
    long Date,
    string Body,
    int Read,
    int MsgBox,
    string ReadableDate,
    string ContactName,
    IReadOnlyList<MmsPart> Parts) : IMessage
{
    public IReadOnlyList<string> Addrs { get; init; } = [];
    public bool IsSent => MsgBox == 2;
    public bool IsReceived => !IsSent;
    public string DisplayBody => string.IsNullOrEmpty(Body) ? "[Media message]" : Body;
}
