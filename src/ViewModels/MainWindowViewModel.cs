using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SmsViewer.Models;
using SmsViewer.Repositories;
using SmsViewer.Services;

namespace SmsViewer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ISmsRepository _repository;
    private readonly IFilePickerService _filePickerService;
    private bool _isLoading;
    private string? _errorMessage;

    /// <summary>Parameterless constructor for Avalonia designer support.</summary>
    public MainWindowViewModel()
    {
        _repository = null!;
        _filePickerService = null!;
        OpenXmlFileCommand = null!;
    }

    public MainWindowViewModel(ISmsRepository repository, IFilePickerService filePickerService)
    {
        _repository = repository;
        _filePickerService = filePickerService;
        OpenXmlFileCommand = new AsyncRelayCommand(OpenXmlFileAsync);
    }

    public ObservableCollection<IMessage> Messages { get; } = new();

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

    private async Task OpenXmlFileAsync()
    {
        var filePath = await _filePickerService.PickXmlFileAsync();
        if (string.IsNullOrEmpty(filePath)) return;

        IsLoading = true;
        ErrorMessage = null;
        Messages.Clear();

        try
        {
            await using var stream = File.OpenRead(filePath);
            await foreach (var message in _repository.GetMessagesAsync(stream))
            {
                Messages.Add(message);
            }
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
