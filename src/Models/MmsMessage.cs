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
    string ReadableDate,
    string ContactName,
    IReadOnlyList<MmsPart> Parts) : IMessage;
