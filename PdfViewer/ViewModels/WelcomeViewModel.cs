using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PdfViewer.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;


namespace PdfViewer.ViewModels;

public partial class WelcomeViewModel : ObservableObject
{    
    private readonly IPdfDocumentService _pdfDocumentService;

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
    
    public IAsyncRelayCommand ShowZoomCommand { get; }

    public WelcomeViewModel(IPdfDocumentService pdfDocumentService)
    {        
        _pdfDocumentService = pdfDocumentService;

        OpenPdfCommand = new AsyncRelayCommand(OpenPdfAsync);
        SaveSelectedCommand = new RelayCommand(SaveSelected, CanOperateOnSelected);
        RasterizeSelectedCommand = new AsyncRelayCommand(RasterizeSelected, CanOperateOnSelected);
        DeleteSelectedCommand = new RelayCommand(DeleteSelected, CanOperateOnSelected);
        ShowZoomCommand = new AsyncRelayCommand<PdfPageViewModel>(ShowZoomAsync);

        Pages.CollectionChanged += (s, e) =>
        {
            foreach (var page in Pages)
                page.PropertyChanged += (s2, e2) =>
                {
                    if (e2.PropertyName == nameof(PdfPageViewModel.IsSelected))
                        UpdateCommands();
                };
        };
        _pdfDocumentService = pdfDocumentService;
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

            var pdfDoc = await _pdfDocumentService.Rasterize(dialog.FileName);
            Pages.Clear();
            for (int i = 0; i < pdfDoc.Count; i++)
            {
                var vm = new PdfPageViewModel
                {
                    PageNumber = i + 1,
                    PageThumbnail = pdfDoc.ElementAt(i),
                    IsSelected = false
                };

                vm.PropertyChanged += (s2, e2) =>
                {
                    if (e2.PropertyName == nameof(PdfPageViewModel.IsSelected))
                        UpdateCommands();
                };
                Pages.Add(vm);
            }
            UpdateCommands();
            IsLoading = false;
        }
    }
    private async Task RasterizeSelected()
    {
        IsLoading = true;
        LoadingStatusText = "Идет процесс обработки файла PDF...";
        if (FileSource is null || string.IsNullOrEmpty(FileSource))
        {
            MessageBox.Show("Ошибка при попытке загрузить страницу PDF файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
       
        var dialog = new SaveFileDialog
        {
            Filter = "PDF файлы (*.pdf)|*.pdf"
        };
        
        if (dialog.ShowDialog() == true)
        {
            IsLoading = true;
            LoadingStatusText = "Сохраняю файл PDF...";


            var pageNumbers = Pages
                .Where(p => p.IsSelected)
                .Select(x => x.PageNumber)
                .ToArray();

            var outputPath = dialog.FileName;
            var rasterizedPages = await _pdfDocumentService.RasterizeByPages(pageNumbers, FileSource);
            var result = await _pdfDocumentService.ImagesToPdf(rasterizedPages, dialog.FileName);
            if (!result)
            {
                MessageBox.Show("Произошла ошибка при попытке сохранения PDF файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBox.Show("Файл успешо сохранен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            IsLoading = false;
            //try
            //{



            //    await Task.Run(() =>
            //    {
            //        var outputPath = dialog.FileName;

            //        using var document = PdfDocument.Load(FileSource);
            //        using var pdf = new PdfSharp.Pdf.PdfDocument();
            //        foreach (var page in pagesToRasterize)
            //        {
            //            int zeroBased = page.PageNumber - 1;
            //            if(zeroBased < 0 ||  zeroBased >= document.PageCount)
            //                continue;
            //            try
            //            {

            //                var pageSize = document.PageSizes[zeroBased];
            //                int dpi = 200;
            //                int widthPx = (int)Math.Round(pageSize.Width / 72.0 * dpi);
            //                int heightPx = (int)Math.Round(pageSize.Height / 72.0 * dpi);

            //                using var image = document.Render(zeroBased, widthPx, heightPx, dpi, dpi, PdfRenderFlags.ForPrinting);



            //                var pdfPage = pdf.AddPage();
            //                pdfPage.Width = XUnit.FromPoint(image.Width * 72.0 / 200.0);
            //                pdfPage.Height = XUnit.FromPoint(image.Height * 72.0 / 200.0);

            //                using var gfx = XGraphics.FromPdfPage(pdfPage);
            //                using var ms = new MemoryStream();
            //                image.Save(ms, ImageFormat.Png);
            //                ms.Position = 0;
            //                var xImage = XImage.FromStream(ms);
            //                gfx.DrawImage(xImage, new XRect(0, 0, pdfPage.Width, pdfPage.Height));
            //            }
            //            catch (Exception e)
            //            {
            //                MessageBox.Show(e.Message + " " + e.HelpLink);
            //            }
            //        }

            //        try
            //        {
            //            pdf.Save(outputPath);
            //            MessageBox.Show("Файл сохранен успешно", "Успешно", MessageBoxButton.OK,
            //                MessageBoxImage.Information);
            //        }
            //        catch (Exception e)
            //        {
            //            MessageBox.Show(e.Message, "Ошибка", MessageBoxButton.OK,
            //                MessageBoxImage.Error);
            //        }
            //    });
            //}
            //finally
            //{
            //    IsLoading = false;
            //}
        }
        else
        {
            IsLoading = false;
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

    private async Task ShowZoomAsync(PdfPageViewModel? page)
    {
        if (page is null)
            return;

        IsLoading = true;
        LoadingStatusText = "Открываю страницу, ожидайте...";
        
        var pageIndex = page.PageNumber;

        if(FileSource is null || string.IsNullOrEmpty(FileSource))
        {
            MessageBox.Show("Ошибка при попытке загрузить страницу PDF файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var imgByGS = await _pdfDocumentService.RasterizeByPage(pageIndex, FileSource);
        var bmp = new BitmapImage();
        using (var fileStream = new FileStream(imgByGS, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose | FileOptions.Asynchronous))
        {
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = fileStream;
            bmp.EndInit();
            bmp.Freeze();
        }        
        var window = new Views.PagePreviewView(bmp);
        window.Show();
        IsLoading = false;        
    }
}
