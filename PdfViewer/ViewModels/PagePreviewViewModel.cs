using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PdfViewer.Helpers;
using PdfViewer.Services;

namespace PdfViewer.ViewModels;

public partial class PagePreviewViewModel : ObservableObject
{
    [ObservableProperty]
    private ImageSource? fullPageImage;
    
    [ObservableProperty] 
    private double scale = 1.0;
    
    [ObservableProperty] 
    private double offsetX = 0;
    
    [ObservableProperty] 
    private double offsetY = 0;
    
    [ObservableProperty] 
    private double angle = 0;
    
    [ObservableProperty]
    private bool toolbarVisible = true;

    [ObservableProperty] 
    private string printStatus = string.Empty;
    
    [ObservableProperty] private Visibility printStatusVisibility = Visibility.Collapsed;

    
    private bool _isDragging;
    private Point _dragStartPoint;
    private Vector _dragStartOffset;
    private IPdfDocumentService _documentService;

    public PagePreviewViewModel(ImageSource? image)
    {
        FullPageImage = image;

        ZoomInCommand = new RelayCommand(() => Scale *= 1.25);
        ZoomOutCommand = new RelayCommand(() => Scale = Math.Max(1.0, Scale * 0.8));
        ResetZoomCommand = new RelayCommand(ResetView);
        RotateLeftCommand = new RelayCommand(() => Angle = (Angle - 90) % 360);
        RotateRightCommand = new RelayCommand(() => Angle = (Angle + 90) % 360);
        PrintCommand = new AsyncRelayCommand(PrintImage);
        SaveAsPdfCommand = new RelayCommand(SaveImageAsPdf);
        _documentService = new PdfDocumentService();
    }
    
    public IRelayCommand ZoomInCommand { get; }
    public IRelayCommand ZoomOutCommand { get; }
    public IRelayCommand ResetZoomCommand { get; }
    public IRelayCommand RotateLeftCommand { get; }
    public IRelayCommand RotateRightCommand { get; }
    public IRelayCommand PrintCommand { get; }
    public IRelayCommand SaveAsPdfCommand { get; }

  public void StartDrag(Point mousePoint)
    {
        if (Scale <= 1.01) return;
        _isDragging = true;
        _dragStartPoint = mousePoint;
        _dragStartOffset = new Vector(OffsetX, OffsetY);
    }

    public void UpdateDrag(Point mousePoint)
    {
        if (!_isDragging) return;
        var delta = mousePoint - _dragStartPoint;
        OffsetX = _dragStartOffset.X + delta.X;
        OffsetY = _dragStartOffset.Y + delta.Y;
    }

    public void StopDrag()
    {
        _isDragging = false;
    }

    public void MouseWheelZoom(int delta)
    {
        if (delta > 0)
            Scale *= 1.25;
        else
            Scale = Math.Max(1.0, Scale * 0.8);

        if (Scale <= 1.0)
        {
            OffsetX = 0;
            OffsetY = 0;
        }
    }

    private void ResetView()
    {
        Scale = 1.0;
        OffsetX = 0;
        OffsetY = 0;
        Angle = 0;
    }

    public void ShowToolbar()
    {
        ToolbarVisible = true;
    }

    public void HideToolbar()
    {
        ToolbarVisible = false;
    }

    private async Task PrintImage()
    {
        if(FullPageImage is null) return;
        
        if (FullPageImage is BitmapSource bmp)
        {
            var format = PaperFormatHelper.DetectPaperFormat(bmp, out double width, out double height);
            await ShowPrintStatus($"Формат печати: {format} ({width:F0} x {height:F0} мм)");
            string result = PrintingHelper.PrintImageSource(
                imageSource: FullPageImage,
                printerName: "Microsoft Print to PDF",
                widthMm: fullPageImage.Width,
                heightMm: fullPageImage.Height,
                onStatusUpdate: msg => printStatus = msg);
            
            await ShowPrintStatus(result);
        }
    }
    
    private async Task ShowPrintStatus(string message)
    {
        PrintStatus = message;
        PrintStatusVisibility = Visibility.Visible;

        await Task.Delay(5000);
        
        if (PrintStatus == message)
            PrintStatusVisibility = Visibility.Collapsed;
    }

    private void SaveImageAsPdf()
    {
        Application.Current.Dispatcher.Invoke(async () =>
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "PDF файл (*.pdf)|*.pdf",
                FileName = "Мой документ.pdf"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var bitmapSource = FullPageImage as BitmapSource;
                    if (bitmapSource == null)
                    {
                        MessageBox.Show("Изображение не найдено.");
                        return;
                    }
                    var encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    using var ms = new MemoryStream();
                    encoder.Save(ms);
                    //ms.Position = 0;
                    string tmpImgPath = Path.GetTempFileName() + ".jpg";
                    await File.WriteAllBytesAsync(tmpImgPath, 
                        ms.ToArray());
                    string tmpPdfName = Path.GetTempFileName() + ".pdf";
                    var result = _documentService.ImageToPdf(tmpImgPath, tmpPdfName);
                    if(result)
                    {
                        File.Copy(tmpPdfName, dlg.FileName, true);
                        File.Delete(tmpPdfName);
                        MessageBox.Show("Сохранено как PDF!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }                        
                    else
                        MessageBox.Show("Ошибка сохранения PDF ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения PDF: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        });
    }
}