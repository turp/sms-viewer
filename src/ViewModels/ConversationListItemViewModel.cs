using SmsViewer.Models;

namespace SmsViewer.ViewModels;

public class ConversationListItemViewModel : ViewModelBase
{
    private bool _isSelected;

    public string Address { get; }
    public string DisplayName { get; }
    public string LastMessagePreview { get; }
    public string LastMessageDate { get; }
    public long LastMessageDateUnixMs { get; }
    public int MessageCount { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public ConversationListItemViewModel(ConversationSummary summary)
    {
        Address = summary.Address;
        DisplayName = summary.DisplayName;
        LastMessagePreview = summary.LastMessagePreview;
        LastMessageDate = summary.LastMessageDate;
        LastMessageDateUnixMs = summary.LastMessageDateUnixMs;
        MessageCount = summary.MessageCount;
    }
}
