using System;
using System.Collections.Generic;
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
    private readonly IUpdateService? _updateService;
    private bool _isLoading;
    private bool _isThreadLoading;
    private string? _errorMessage;
    private bool _updateAvailable;
    private string? _availableVersion;
    private ThemeDefinition _activeTheme = ThemeService.Current;
    private ConversationListItemViewModel? _selectedConversation;
    private string _searchText = string.Empty;
    private string _filterFromDate = string.Empty;
    private string _filterToDate = string.Empty;
    private string _threadSearchText = string.Empty;
    private bool _conversationsLoaded;
    private string? _currentFilePath;
    private List<IMessage> _loadedMessages = new();

    /// <summary>Parameterless constructor for Avalonia designer support.</summary>
    public MainWindowViewModel()
    {
        _conversationService = null!;
        _filePickerService = null!;
        OpenXmlFileCommand = null!;
        RestartToUpdateCommand = null!;
        DismissUpdateCommand = null!;
    }

    public MainWindowViewModel(IConversationService conversationService, IFilePickerService filePickerService, IUpdateService? updateService = null)
    {
        _conversationService = conversationService;
        _filePickerService = filePickerService;
        _updateService = updateService;
        OpenXmlFileCommand = new AsyncRelayCommand(OpenXmlFileAsync);
        RestartToUpdateCommand = new AsyncRelayCommand(RestartToUpdateAsync);
        DismissUpdateCommand = new RelayCommand(() => UpdateAvailable = false);

        if (_updateService != null)
            UpdateCheckTask = RunUpdateCheckAsync();
    }

    public IReadOnlyList<ThemeDefinition> AvailableThemes => ThemeService.AvailableThemes;

    public ThemeDefinition ActiveTheme
    {
        get => _activeTheme;
        set
        {
            if (SetProperty(ref _activeTheme, value) && value != null)
                ThemeService.Apply(value);
        }
    }

    public ObservableCollection<ConversationListItemViewModel> Conversations { get; } = new();
    public ObservableCollection<ConversationListItemViewModel> FilteredConversations { get; } = new();
    public ObservableCollection<MessageViewModel> FilteredMessages { get; } = new();

    public IAsyncRelayCommand OpenXmlFileCommand { get; }
    public IAsyncRelayCommand RestartToUpdateCommand { get; }
    public IRelayCommand DismissUpdateCommand { get; }

    /// <summary>Exposed so tests can await the async update check triggered on construction.</summary>
    public Task? UpdateCheckTask { get; private set; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool IsThreadLoading
    {
        get => _isThreadLoading;
        set => SetProperty(ref _isThreadLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool UpdateAvailable
    {
        get => _updateAvailable;
        set => SetProperty(ref _updateAvailable, value);
    }

    public string? AvailableVersion
    {
        get => _availableVersion;
        set => SetProperty(ref _availableVersion, value);
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
                ThreadLoadTask = LoadSelectedThreadAsync();
            }
        }
    }

    /// <summary>Exposed so tests can await the async thread load triggered by SelectedConversation changes.</summary>
    public Task? ThreadLoadTask { get; private set; }

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
        SelectedConversation != null && !IsThreadLoading && FilteredMessages.Count == 0;

    private async Task RunUpdateCheckAsync()
    {
        var hasUpdate = await _updateService!.CheckForUpdateAsync();
        if (hasUpdate)
        {
            AvailableVersion = _updateService.AvailableVersion;
            UpdateAvailable = true;
        }
    }

    private Task RestartToUpdateAsync() =>
        _updateService?.ApplyUpdateAndRestartAsync() ?? Task.CompletedTask;

    private async Task OpenXmlFileAsync()
    {
        var filePath = await _filePickerService.PickXmlFileAsync();
        if (string.IsNullOrEmpty(filePath)) return;

        IsLoading = true;
        ErrorMessage = null;
        _conversationsLoaded = false;
        _currentFilePath = null;

        // Reset filter state without triggering cascading filter calls
        _searchText = string.Empty;       OnPropertyChanged(nameof(SearchText));
        _filterFromDate = string.Empty;   OnPropertyChanged(nameof(FilterFromDate));
        _filterToDate = string.Empty;     OnPropertyChanged(nameof(FilterToDate));
        _threadSearchText = string.Empty; OnPropertyChanged(nameof(ThreadSearchText));

        Conversations.Clear();
        FilteredConversations.Clear();
        FilteredMessages.Clear();
        _loadedMessages.Clear();
        _selectedConversation = null;
        OnPropertyChanged(nameof(SelectedConversation));

        try
        {
            await using var stream = File.OpenRead(filePath);
            var summaries = await _conversationService.GetConversationSummariesAsync(stream);
            foreach (var s in summaries)
                Conversations.Add(new ConversationListItemViewModel(s));
            _currentFilePath = filePath;
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

    private async Task LoadSelectedThreadAsync()
    {
        FilteredMessages.Clear();
        _loadedMessages.Clear();
        if (_selectedConversation == null || _currentFilePath == null) return;

        IsThreadLoading = true;
        try
        {
            await using var stream = File.OpenRead(_currentFilePath);
            var messages = await _conversationService.GetConversationMessagesAsync(stream, _selectedConversation.Address);
            _loadedMessages = messages.ToList();
            ApplyMessageFilter();
        }
        finally
        {
            IsThreadLoading = false;
            OnPropertyChanged(nameof(HasNoMessageResults));
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
            ? _loadedMessages
            : _loadedMessages.Where(m => m.DisplayBody.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var m in source)
            FilteredMessages.Add(new MessageViewModel(m));

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
