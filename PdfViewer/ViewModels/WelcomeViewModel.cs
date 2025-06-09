
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PdfViewer.Services;
using System.Collections.ObjectModel;

namespace PdfViewer.ViewModels;

public partial class WelcomeViewModel : ObservableObject
{
    private readonly IPdfThumbnailService _pdfService;

    [ObservableProperty]
    private string _greetings = "Добро пожаловать!";

    [ObservableProperty]
    private string? filename;

    [ObservableProperty]
    private bool isLoading;


    [ObservableProperty]
    private ObservableCollection<PdfPageViewModel> pages = new();

    public IRelayCommand OpenPdfCommand { get; }
    public IRelayCommand SaveSelectedCommand { get; }
    public IRelayCommand RasterizeSelectedCommand { get; }
    public IRelayCommand DeleteSelectedCommand { get; }

    public WelcomeViewModel(IPdfThumbnailService pdfThumbnailService)
    {
        _pdfService = pdfThumbnailService;

        OpenPdfCommand = new AsyncRelayCommand(OpenPdfAsync);
        SaveSelectedCommand = new RelayCommand(SaveSelected, CanOperateOnSelected);
        RasterizeSelectedCommand = new RelayCommand(RasterizeSelected, CanOperateOnSelected);
        DeleteSelectedCommand = new RelayCommand(DeleteSelected, CanOperateOnSelected);

        Pages.CollectionChanged += (s, e) =>
        {
            foreach (var page in Pages)
                page.PropertyChanged += (s2, e2) =>
                {
                    if (e2.PropertyName == nameof(PdfPageViewModel.IsSelected))
                        UpdateCommands();
                };
        };
    }

    private void UpdateCommands()
    {
        SaveSelectedCommand.NotifyCanExecuteChanged();
        RasterizeSelectedCommand.NotifyCanExecuteChanged();
        DeleteSelectedCommand.NotifyCanExecuteChanged();
    }

    private bool CanOperateOnSelected() => Pages.Any(p => p.IsSelected);
    
    private async Task OpenPdfAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "PDF файлы (*.pdf)|*.pdf"
        };

        if (dialog.ShowDialog() == true)
        {
            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    var pdf = _pdfService.LoadAndRasterize(dialog.FileName);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Filename = pdf.Filename;
                        Pages.Clear();
                        foreach (var page in pdf.Pages)
                        {

                            var vm = new PdfPageViewModel
                            {
                                PageNumber = page.PageNumber,
                                PageThumbnail = page.PageThumbnail,
                                IsSelected = false
                            };

                            vm.PropertyChanged += (s, e) =>
                            {
                                if (e.PropertyName == nameof(PdfPageViewModel.IsSelected))
                                    UpdateCommands();
                            };
                            Pages.Add(vm);
                        }
                        UpdateCommands();
                    });
                });
            }
            finally 
            {
                IsLoading = false;
            }
            
        }
    }

    private void SaveSelected()
    {
        // TODO: Реализуйте сохранение выбранных страниц
        // Пример: Pages.Where(p => p.IsSelected)
    }

    private void RasterizeSelected()
    {
        // TODO: Реализуйте растеризацию выбранных страниц
    }

    private void DeleteSelected()
    {
        var toDelete = Pages.Where(p => p.IsSelected).ToList();
        foreach (var page in toDelete)
            Pages.Remove(page);
        UpdateCommands();
    }
}
