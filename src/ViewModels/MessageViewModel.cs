using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SmsViewer.Models;

namespace SmsViewer.ViewModels;

public sealed class MessageViewModel
{
    private static readonly Regex UrlRegex = new(@"https?://\S+", RegexOptions.Compiled);

    public MessageViewModel(IMessage message)
    {
        IsSent = message.IsSent;
        IsReceived = message.IsReceived;
        ReadableDate = message.ReadableDate;
        Body = message.Body;
        DisplayBody = message.DisplayBody;
        BodySegments = ParseSegments(message.DisplayBody);
        ImageData = DecodeImages(message);
        HasImages = ImageData.Count > 0;
    }

    public bool IsSent { get; }
    public bool IsReceived { get; }
    public string ReadableDate { get; }
    public string Body { get; }
    public string DisplayBody { get; }
    public IReadOnlyList<BodySegment> BodySegments { get; }
    /// <summary>Raw bytes for each embedded image, decoded from base64 MMS parts.</summary>
    public IReadOnlyList<byte[]> ImageData { get; }
    public bool HasImages { get; }

    private static IReadOnlyList<BodySegment> ParseSegments(string text)
    {
        if (string.IsNullOrEmpty(text))
            return [new BodySegment(false, text)];

        var segments = new List<BodySegment>();
        int lastIndex = 0;

        foreach (Match match in UrlRegex.Matches(text))
        {
            if (match.Index > lastIndex)
                segments.Add(new BodySegment(false, text[lastIndex..match.Index]));
            segments.Add(new BodySegment(true, match.Value));
            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < text.Length)
            segments.Add(new BodySegment(false, text[lastIndex..]));

        return segments.Count > 0 ? segments : [new BodySegment(false, text)];
    }

    private static IReadOnlyList<byte[]> DecodeImages(IMessage message)
    {
        if (message is not MmsMessage mms) return [];

        var result = new List<byte[]>();
        foreach (var part in mms.Parts)
        {
            if (!part.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.IsNullOrEmpty(part.Data)) continue;
            try { result.Add(Convert.FromBase64String(part.Data)); }
            catch { /* skip malformed base64 */ }
        }
        return result;
    }
}
