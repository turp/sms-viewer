using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SmsViewer.Services;

namespace SmsViewer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IConversationService _conversationService;
    private readonly IFilePickerService _filePickerService;
    private bool _isLoading;
    private string? _errorMessage;
    private ConversationListItemViewModel? _selectedConversation;

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
        set => SetProperty(ref _selectedConversation, value);
    }

    private async Task OpenXmlFileAsync()
    {
        var filePath = await _filePickerService.PickXmlFileAsync();
        if (string.IsNullOrEmpty(filePath)) return;

        IsLoading = true;
        ErrorMessage = null;
        Conversations.Clear();
        SelectedConversation = null;

        try
        {
            await using var stream = File.OpenRead(filePath);
            var conversations = await _conversationService.GetConversationsAsync(stream);
            foreach (var c in conversations)
                Conversations.Add(new ConversationListItemViewModel(c));
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
        }
    }
}
