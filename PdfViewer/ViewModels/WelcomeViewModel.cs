
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PdfViewer.Services;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using PdfiumViewer;
using PdfSharp.Drawing;
using PdfViewer.Models;

namespace PdfViewer.ViewModels;

public partial class WelcomeViewModel : ObservableObject
{
    private readonly IPdfThumbnailService _pdfService;

    [ObservableProperty]
    private string _greetings = "Добро пожаловать!";

    [ObservableProperty]
    private string? _fileName;
    
    [ObservableProperty]
    private string? _fileSource;

    [ObservableProperty]
    private bool isLoading;
    
    [ObservableProperty]
    private string loadingStatusText;


    [ObservableProperty]
    private ObservableCollection<PdfPageViewModel> pages = new();

    public IRelayCommand OpenPdfCommand { get; }
    public IRelayCommand SaveSelectedCommand { get; }
    public IRelayCommand RasterizeSelectedCommand { get; }
    public IRelayCommand DeleteSelectedCommand { get; }
    
    public IRelayCommand ShowZoomCommand { get; }

    public WelcomeViewModel(IPdfThumbnailService pdfThumbnailService)
    {
        _pdfService = pdfThumbnailService;

        OpenPdfCommand = new AsyncRelayCommand(OpenPdfAsync);
        SaveSelectedCommand = new RelayCommand(SaveSelected, CanOperateOnSelected);
        RasterizeSelectedCommand = new AsyncRelayCommand(RasterizeSelected, CanOperateOnSelected);
        DeleteSelectedCommand = new RelayCommand(DeleteSelected, CanOperateOnSelected);
        ShowZoomCommand = new RelayCommand<PdfPageViewModel>(ShowZoom);

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
            LoadingStatusText = "Загружаю файл PDF...";
            FileSource = dialog.FileName;
            try
            {
                await Task.Run(() =>
                {
                    var pdf = _pdfService.LoadAndRasterize(dialog.FileName);
                    FileName = pdf.Filename;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        
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
    private async Task RasterizeSelected()
    {
        var pagesToRasterize = Pages.Where(p => p.IsSelected).ToList();
        var dialog = new SaveFileDialog
        {
            Filter = "PDF файлы (*.pdf)|*.pdf"
        };
        
        if (dialog.ShowDialog() == true)
        {
            IsLoading = true;
            LoadingStatusText = "Сохраняю файл PDF...";

            try
            {
                await Task.Run(() =>
                {
                    var outputPath = dialog.FileName;

                    using var document = PdfDocument.Load(FileSource);
                    using var pdf = new PdfSharp.Pdf.PdfDocument();
                    foreach (var page in pagesToRasterize)
                    {
                        int zeroBased = page.PageNumber - 1;
                        if(zeroBased < 0 ||  zeroBased >= document.PageCount)
                            continue;
                        try
                        {
                
                            var pageSize = document.PageSizes[zeroBased];
                            int dpi = 200;
                            int widthPx = (int)Math.Round(pageSize.Width / 72.0 * dpi);
                            int heightPx = (int)Math.Round(pageSize.Height / 72.0 * dpi);

                            using var image = document.Render(zeroBased, widthPx, heightPx, dpi, dpi, PdfRenderFlags.ForPrinting);
                
                          
                            
                            var pdfPage = pdf.AddPage();
                            pdfPage.Width = XUnit.FromPoint(image.Width * 72.0 / 200.0);
                            pdfPage.Height = XUnit.FromPoint(image.Height * 72.0 / 200.0);

                            using var gfx = XGraphics.FromPdfPage(pdfPage);
                            using var ms = new MemoryStream();
                            image.Save(ms, ImageFormat.Png);
                            ms.Position = 0;
                            var xImage = XImage.FromStream(ms);
                            gfx.DrawImage(xImage, new XRect(0, 0, pdfPage.Width, pdfPage.Height));
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message + " " + e.HelpLink);
                        }
                    }

                    try
                    {
                        pdf.Save(outputPath);
                        MessageBox.Show("Файл сохранен успешно", "Успешно", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Ошибка", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
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

    private void DeleteSelected()
    {
        var toDelete = Pages.Where(p => p.IsSelected).ToList();
        foreach (var page in toDelete)
            Pages.Remove(page);
        UpdateCommands();
    }
    
    private void ShowZoom(PdfPageViewModel? page)
    {
        if (page is null) return;
            IsLoading = true;
        
        LoadingStatusText = "Открываю страницу ожидайте...";
           
        var dpi = 200;
        var pageIndex = page.PageNumber - 1;
        
        using var document = PdfDocument.Load(FileSource);
        
        var pageSize = document.PageSizes[pageIndex];
        int widthPx = (int)Math.Round(pageSize.Width / 72.0 * dpi);
        int heightPx = (int)Math.Round(pageSize.Height / 72.0 * dpi);
        using var image = document.Render(pageIndex, widthPx, heightPx, dpi, dpi, PdfRenderFlags.ForPrinting);
        
        using var ms = new MemoryStream();
        image.Save(ms, ImageFormat.Png);
        ms.Position = 0;

        // Создаём ImageSource для WPF
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = ms;
        bitmap.EndInit();
        bitmap.Freeze();
        var window = new Views.PagePreviewView(bitmap);
        window.Show();
        IsLoading = false;
    }
}
