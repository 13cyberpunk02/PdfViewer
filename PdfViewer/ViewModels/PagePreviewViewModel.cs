using System.IO;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

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
    
    private bool _isDragging;
    private Point _dragStartPoint;
    private Vector _dragStartOffset;

    public PagePreviewViewModel(ImageSource? image)
    {
        FullPageImage = image;

        ZoomInCommand = new RelayCommand(() => Scale *= 1.25);
        ZoomOutCommand = new RelayCommand(() => Scale = Math.Max(1.0, Scale * 0.8));
        ResetZoomCommand = new RelayCommand(ResetView);
        RotateLeftCommand = new RelayCommand(() => Angle = (Angle - 90) % 360);
        RotateRightCommand = new RelayCommand(() => Angle = (Angle + 90) % 360);
        PrintCommand = new RelayCommand(PrintImage);
        SaveAsPdfCommand = new RelayCommand(SaveImageAsPdf);
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

    public void ResetView()
    {
        Scale = 1.0;
        OffsetX = 0;
        OffsetY = 0;
        Angle = 0;
    }

    // Toolbar logic
    public void ShowToolbar()
    {
        ToolbarVisible = true;
    }

    public void HideToolbar()
    {
        ToolbarVisible = false;
    }

    // Print
    private void PrintImage()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var dlg = new System.Windows.Controls.PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                var vis = new System.Windows.Controls.Image
                {
                    Source = FullPageImage,
                    Width = FullPageImage?.Width ?? 0,
                    Height = FullPageImage?.Height ?? 0,
                    Stretch = Stretch.Uniform
                };
                dlg.PrintVisual(vis, "Печать изображения");
            }
        });
    }

    // Save as PDF
    private void SaveImageAsPdf()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "PDF файл (*.pdf)|*.pdf",
                FileName = "Изображение.pdf"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var bitmapSource = FullPageImage as System.Windows.Media.Imaging.BitmapSource;
                    if (bitmapSource == null)
                    {
                        MessageBox.Show("Изображение не найдено.");
                        return;
                    }
                    using (var ms = new MemoryStream())
                    {
                        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapSource));
                        encoder.Save(ms);
                        ms.Position = 0;

                        using (var pdf = new PdfDocument())
                        {
                            var page = pdf.AddPage();
                            using (var gfx = XGraphics.FromPdfPage(page))
                            using (var img = XImage.FromStream(ms))
                            {
                                page.Width = img.PixelWidth * 72.0 / img.HorizontalResolution;
                                page.Height = img.PixelHeight * 72.0 / img.VerticalResolution;
                                gfx.DrawImage(img, 0, 0, page.Width, page.Height);
                            }
                            pdf.Save(dlg.FileName);
                        }
                    }
                    MessageBox.Show("Сохранено как PDF!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения PDF: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        });
    }
}