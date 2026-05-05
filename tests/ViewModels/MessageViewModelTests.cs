using System.Collections.Generic;
using SmsViewer.Models;
using SmsViewer.ViewModels;

namespace SmsViewer.Tests.ViewModels;

public class MessageViewModelTests
{
    private static SmsMessage Sms(string body, int type = 1) =>
        new("111", 0, type, body, 1, -1, "Jan 1, 2026", "Alice");

    // ── Body segment parsing ──────────────────────────────────────────────

    [Fact]
    public void PlainText_ProducesOneNonUrlSegment()
    {
        var vm = new MessageViewModel(Sms("Hello world"));
        Assert.Single(vm.BodySegments);
        Assert.False(vm.BodySegments[0].IsUrl);
        Assert.Equal("Hello world", vm.BodySegments[0].Text);
    }

    [Fact]
    public void UrlOnly_ProducesOneUrlSegment()
    {
        var vm = new MessageViewModel(Sms("https://example.com"));
        Assert.Single(vm.BodySegments);
        Assert.True(vm.BodySegments[0].IsUrl);
        Assert.Equal("https://example.com", vm.BodySegments[0].Text);
    }

    [Fact]
    public void TextThenUrl_ProducesTwoSegments()
    {
        var vm = new MessageViewModel(Sms("Check https://example.com"));
        Assert.Equal(2, vm.BodySegments.Count);
        Assert.False(vm.BodySegments[0].IsUrl);
        Assert.Equal("Check ", vm.BodySegments[0].Text);
        Assert.True(vm.BodySegments[1].IsUrl);
        Assert.Equal("https://example.com", vm.BodySegments[1].Text);
    }

    [Fact]
    public void TextUrlText_ProducesThreeSegments()
    {
        var vm = new MessageViewModel(Sms("Go to https://example.com for info"));
        Assert.Equal(3, vm.BodySegments.Count);
        Assert.False(vm.BodySegments[0].IsUrl);
        Assert.Equal("Go to ", vm.BodySegments[0].Text);
        Assert.True(vm.BodySegments[1].IsUrl);
        Assert.Equal("https://example.com", vm.BodySegments[1].Text);
        Assert.False(vm.BodySegments[2].IsUrl);
        Assert.Equal(" for info", vm.BodySegments[2].Text);
    }

    [Fact]
    public void TwoUrls_ProducesFourSegments()
    {
        // "See " + "https://a.com" + " and " + "https://b.com" = 4 segments
        var vm = new MessageViewModel(Sms("See https://a.com and https://b.com"));
        Assert.Equal(4, vm.BodySegments.Count);
        Assert.True(vm.BodySegments[1].IsUrl);
        Assert.Equal("https://a.com", vm.BodySegments[1].Text);
        Assert.True(vm.BodySegments[3].IsUrl);
        Assert.Equal("https://b.com", vm.BodySegments[3].Text);
    }

    [Fact]
    public void EmptyBody_ProducesOneEmptySegment()
    {
        var vm = new MessageViewModel(Sms(""));
        Assert.Single(vm.BodySegments);
        Assert.False(vm.BodySegments[0].IsUrl);
        Assert.Equal("", vm.BodySegments[0].Text);
    }

    [Fact]
    public void HttpAndHttpsUrls_BothDetected()
    {
        var vm = new MessageViewModel(Sms("http://a.com and https://b.com"));
        var urlSegments = vm.BodySegments.Where(s => s.IsUrl).ToList();
        Assert.Equal(2, urlSegments.Count);
    }

    // ── IsSent / passthrough ──────────────────────────────────────────────

    [Fact]
    public void IsSent_ReflectsUnderlyingMessage()
    {
        Assert.True(new MessageViewModel(Sms("hi", type: 2)).IsSent);
        Assert.False(new MessageViewModel(Sms("hi", type: 1)).IsSent);
    }

    [Fact]
    public void Body_ExposesRawBody()
    {
        var vm = new MessageViewModel(Sms("hello"));
        Assert.Equal("hello", vm.Body);
    }

    [Fact]
    public void DisplayBody_ExposesDisplayBody()
    {
        var parts = new List<MmsPart>();
        var mms = new MmsMessage("111", 0, "", 1, 1, "date", "Alice", parts);
        var vm = new MessageViewModel(mms);
        Assert.Equal("[Media message]", vm.DisplayBody);
    }

    // ── Image loading ─────────────────────────────────────────────────────

    [Fact]
    public void SmsMessage_HasNoImages()
    {
        var vm = new MessageViewModel(Sms("hi"));
        Assert.False(vm.HasImages);
        Assert.Empty(vm.ImageData);
    }

    [Fact]
    public void MmsWithNoImageParts_HasNoImages()
    {
        var parts = new List<MmsPart>
        {
            new("text/plain", "null", "hello", ""),
        };
        var mms = new MmsMessage("111", 0, "hello", 1, 1, "date", "Alice", parts);
        var vm = new MessageViewModel(mms);
        Assert.False(vm.HasImages);
        Assert.Empty(vm.ImageData);
    }

    [Fact]
    public void MmsWithValidPngPart_DecodesImageData()
    {
        // 1×1 transparent PNG
        const string png1x1 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO7Z3i0AAAAASUVORK5CYII=";
        var parts = new List<MmsPart>
        {
            new("image/png", "img.png", "", png1x1),
        };
        var mms = new MmsMessage("111", 0, "", 1, 1, "date", "Alice", parts);
        var vm = new MessageViewModel(mms);
        Assert.True(vm.HasImages);
        Assert.Single(vm.ImageData);
        Assert.NotEmpty(vm.ImageData[0]);
    }

    [Fact]
    public void MmsWithInvalidBase64Image_SkipsAndHasNoImages()
    {
        var parts = new List<MmsPart>
        {
            new("image/jpeg", "bad.jpg", "", "!!!not-valid-base64!!!"),
        };
        var mms = new MmsMessage("111", 0, "", 1, 1, "date", "Alice", parts);
        var vm = new MessageViewModel(mms);
        Assert.False(vm.HasImages);
        Assert.Empty(vm.ImageData);
    }

    [Fact]
    public void MmsWithMixedParts_OnlyImagePartsDecoded()
    {
        const string png1x1 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO7Z3i0AAAAASUVORK5CYII=";
        var parts = new List<MmsPart>
        {
            new("text/plain", "null", "caption", ""),
            new("image/png", "img.png", "", png1x1),
        };
        var mms = new MmsMessage("111", 0, "caption", 1, 1, "date", "Alice", parts);
        var vm = new MessageViewModel(mms);
        Assert.True(vm.HasImages);
        Assert.Single(vm.ImageData);
    }
}
