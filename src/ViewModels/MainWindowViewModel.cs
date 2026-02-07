using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// Primarily for designer support.
    /// </summary>
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

    public ICommand OpenXmlFileCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private async Task OpenXmlFileAsync()
    {
        var filePath = await _filePickerService.PickXmlFileAsync();
        if (string.IsNullOrEmpty(filePath)) return;

        IsLoading = true;
        try
        {
            Messages.Clear();
            await foreach (var message in _repository.GetMessagesAsync(filePath))
            {
                Messages.Add(message);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
