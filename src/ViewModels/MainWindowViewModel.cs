using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SmsViewer.Models;
using SmsViewer.Services;

namespace SmsViewer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IConversationService _conversationService;
    private readonly IFilePickerService _filePickerService;
    private bool _isLoading;
    private string? _errorMessage;
    private ConversationListItemViewModel? _selectedConversation;
    private string _searchText = string.Empty;
    private string _filterFromDate = string.Empty;
    private string _filterToDate = string.Empty;
    private string _threadSearchText = string.Empty;
    private bool _conversationsLoaded;

    /// <summary>Parameterless constructor for Avalonia designer support.</summary>
    public MainWindowViewModel()
    {
        _conversationService = null!;
        _filePickerService = null!;
        OpenXmlFileCommand = null!;
    }

    public MainWindowViewModel(IConversationService conversationService, IFilePickerService filePickerService)
    {
        _conversationService = conversationService;
        _filePickerService = filePickerService;
        OpenXmlFileCommand = new AsyncRelayCommand(OpenXmlFileAsync);
    }

    public ObservableCollection<ConversationListItemViewModel> Conversations { get; } = new();
    public ObservableCollection<ConversationListItemViewModel> FilteredConversations { get; } = new();
    public ObservableCollection<IMessage> FilteredMessages { get; } = new();

    public IAsyncRelayCommand OpenXmlFileCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ConversationListItemViewModel? SelectedConversation
    {
        get => _selectedConversation;
        set
        {
            if (SetProperty(ref _selectedConversation, value))
            {
                _threadSearchText = string.Empty;
                OnPropertyChanged(nameof(ThreadSearchText));
                ApplyMessageFilter();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) ApplyConversationFilter(); }
    }

    public string FilterFromDate
    {
        get => _filterFromDate;
        set { if (SetProperty(ref _filterFromDate, value)) ApplyConversationFilter(); }
    }

    public string FilterToDate
    {
        get => _filterToDate;
        set { if (SetProperty(ref _filterToDate, value)) ApplyConversationFilter(); }
    }

    public string ThreadSearchText
    {
        get => _threadSearchText;
        set { if (SetProperty(ref _threadSearchText, value)) ApplyMessageFilter(); }
    }

    public bool HasNoConversationResults =>
        _conversationsLoaded && FilteredConversations.Count == 0;

    public bool HasNoMessageResults =>
        SelectedConversation != null && FilteredMessages.Count == 0;

    private async Task OpenXmlFileAsync()
    {
        var filePath = await _filePickerService.PickXmlFileAsync();
        if (string.IsNullOrEmpty(filePath)) return;

        IsLoading = true;
        ErrorMessage = null;
        _conversationsLoaded = false;

        // Reset filter state without triggering cascading filter calls
        _searchText = string.Empty;      OnPropertyChanged(nameof(SearchText));
        _filterFromDate = string.Empty;  OnPropertyChanged(nameof(FilterFromDate));
        _filterToDate = string.Empty;    OnPropertyChanged(nameof(FilterToDate));
        _threadSearchText = string.Empty; OnPropertyChanged(nameof(ThreadSearchText));

        Conversations.Clear();
        FilteredConversations.Clear();
        FilteredMessages.Clear();
        _selectedConversation = null;
        OnPropertyChanged(nameof(SelectedConversation));

        try
        {
            await using var stream = File.OpenRead(filePath);
            var conversations = await _conversationService.GetConversationsAsync(stream);
            foreach (var c in conversations)
                Conversations.Add(new ConversationListItemViewModel(c));
            _conversationsLoaded = true;
            ApplyConversationFilter();
        }
        catch (FileNotFoundException)
        {
            ErrorMessage = $"File not found: {filePath}";
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = $"Access denied: {filePath}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load messages: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(HasNoConversationResults));
        }
    }

    private void ApplyConversationFilter()
    {
        var search = _searchText.Trim();
        var fromMs = ParseDateToMs(_filterFromDate);
        var toMs   = ParseDateToMs(_filterToDate);

        FilteredConversations.Clear();
        foreach (var c in Conversations)
        {
            if (!string.IsNullOrEmpty(search) &&
                !c.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase) &&
                !c.Address.Contains(search, StringComparison.OrdinalIgnoreCase))
                continue;
            if (fromMs.HasValue && c.LastMessageDateUnixMs < fromMs.Value)
                continue;
            if (toMs.HasValue && c.LastMessageDateUnixMs > toMs.Value)
                continue;
            FilteredConversations.Add(c);
        }
        OnPropertyChanged(nameof(HasNoConversationResults));
    }

    private void ApplyMessageFilter()
    {
        FilteredMessages.Clear();
        if (_selectedConversation == null) return;

        var search = _threadSearchText.Trim();
        var source = string.IsNullOrEmpty(search)
            ? _selectedConversation.Messages
            : _selectedConversation.Messages
                .Where(m => m.DisplayBody.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        foreach (var m in source)
            FilteredMessages.Add(m);

        OnPropertyChanged(nameof(HasNoMessageResults));
    }

    private static long? ParseDateToMs(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        if (DateTimeOffset.TryParseExact(
                input.Trim(), "yyyy-MM-dd", null,
                DateTimeStyles.AssumeUniversal,
                out var dt))
            return dt.ToUnixTimeMilliseconds();
        return null;
    }
}
