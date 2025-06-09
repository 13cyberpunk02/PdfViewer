
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PdfViewer.Services;
using System.Collections.ObjectModel;

namespace PdfViewer.ViewModels;

public partial class WelcomeViewModel(IPdfThumbnailService pdfThumbnailService) : ObservableObject
{
    [ObservableProperty]
    private string _greetings = "Добро пожаловать!";

    [ObservableProperty]
    private string? filename;

    [ObservableProperty]
    private bool isLoading;


    [ObservableProperty]
    private ObservableCollection<PdfPageViewModel> pages = new();


    [RelayCommand]
    private async Task OpenPdf()
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
                    var pdf = pdfThumbnailService.LoadAndRasterize(dialog.FileName);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Filename = pdf.Filename;
                        Pages.Clear();
                        foreach (var page in pdf.Pages)
                        {
                            Pages.Add(new PdfPageViewModel
                            {
                                PageNumber = page.PageNumber,
                                PageThumbnail = page.PageThumbnail
                            });
                        }
                    });
                });
            }
            finally 
            {
                IsLoading = false;
            }
            
        }
    }
}
